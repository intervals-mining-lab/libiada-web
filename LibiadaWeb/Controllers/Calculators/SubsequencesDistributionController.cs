namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The subsequences distribution controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesDistributionController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The subsequence extractor.
        /// </summary>
        private readonly SubsequenceExtractor subsequenceExtractor;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// The sequence attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

        /// <summary>
        /// The feature repository.
        /// </summary>
        private readonly FeatureRepository featureRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicRepository characteristicRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesDistributionController"/> class.
        /// </summary>
        public SubsequencesDistributionController()
            : base("Subsequences distribution")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            subsequenceExtractor = new SubsequenceExtractor(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
            sequenceAttributeRepository = new SequenceAttributeRepository(db);
            featureRepository = new FeatureRepository(db);
            characteristicRepository = new CharacteristicRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.GetSubsequencesViewData(1, int.MaxValue, true, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="firstCharacteristicTypeLinkId">
        /// The first characteristic type and link id.
        /// </param>
        /// <param name="secondCharacteristicTypeLinkId">
        /// The second characteristic type and link id.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, int firstCharacteristicTypeLinkId, int secondCharacteristicTypeLinkId, int[] featureIds)
        {
            return Action(() =>
            {
                var sequenceCharacteristics = new List<SequenceData>();
                var newCharacteristics = new List<Characteristic>();

                var parentSequences = db.DnaSequence.Where(s => s.NotationId == Aliases.Notation.Nucleotide && matterIds.Contains(s.MatterId))
                                        .ToDictionary(s => s.Id);
                var parentSequenceIds = parentSequences.Keys.ToArray();
                var allSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceIds, featureIds);
                var subsequenceIds = allSubsequences.SelectMany(s => s).Select(s => s.Id);
                Func<Characteristic, bool> parentCharacteristicFilter = c => parentSequenceIds.Contains(c.SequenceId) && c.CharacteristicTypeLinkId == firstCharacteristicTypeLinkId;
                var parentDbCharacteristics = db.Characteristic.Where(parentCharacteristicFilter).ToList();
                Func<Characteristic, bool> subsequenceCharacteristicFilter = c => subsequenceIds.Contains(c.SequenceId) && c.CharacteristicTypeLinkId == secondCharacteristicTypeLinkId;
                var subsequenceDbCharacteristics = db.Characteristic.Where(subsequenceCharacteristicFilter).ToList();
                var subsequencesDbAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);

                int maxSubsequences = 0;

                foreach (long parentSequenceId in parentSequenceIds)
                {
                    long matterId = parentSequences[parentSequenceId].MatterId;
                    var matterName = db.Matter.Single(m => m.Id == matterId).Name;

                    double sequenceCharacteristicValue;

                    Func<Characteristic, bool> firstCharacteristicFilter = c => c.SequenceId == parentSequenceId && c.CharacteristicTypeLinkId == firstCharacteristicTypeLinkId;

                    if (parentDbCharacteristics.Any(firstCharacteristicFilter))
                    {
                        sequenceCharacteristicValue = parentDbCharacteristics.Single(firstCharacteristicFilter).Value;
                    }
                    else
                    {
                        Chain sequence = commonSequenceRepository.ToLibiadaChain(parentSequenceId);
                        sequence.FillIntervalManagers();
                        string firstClassName = characteristicTypeLinkRepository.GetCharacteristicType(firstCharacteristicTypeLinkId).ClassName;
                        IFullCalculator firstCalculator = CalculatorsFactory.CreateFullCalculator(firstClassName);
                        var firstLink = characteristicTypeLinkRepository.GetLibiadaLink(firstCharacteristicTypeLinkId);
                        sequenceCharacteristicValue = firstCalculator.Calculate(sequence, firstLink);

                        var dataBaseCharacteristic = new Characteristic
                                                         {
                                                             SequenceId = parentSequenceId,
                                                             CharacteristicTypeLinkId = firstCharacteristicTypeLinkId,
                                                             Value = sequenceCharacteristicValue
                                                         };

                        newCharacteristics.Add(dataBaseCharacteristic);
                        parentDbCharacteristics.Add(dataBaseCharacteristic);
                    }

                    List<Subsequence> subsequences = allSubsequences[parentSequenceId].ToList();
                    var libiadaSubsequences = subsequenceExtractor.ExtractChains(subsequences, parentSequenceId);

                    if (maxSubsequences < subsequences.Count)
                    {
                        maxSubsequences = subsequences.Count;
                    }

                    var subsequencesCharacteristics = new List<SubsequenceData>();
                    string secondClassName = characteristicTypeLinkRepository.GetCharacteristicType(secondCharacteristicTypeLinkId).ClassName;
                    IFullCalculator secondCalculator = CalculatorsFactory.CreateFullCalculator(secondClassName);
                    var secondLink = characteristicTypeLinkRepository.GetLibiadaLink(secondCharacteristicTypeLinkId);

                    for (int j = 0; j < subsequences.Count; j++)
                    {
                        double subsequenceCharacteristicValue;

                        Func<Characteristic, bool> secondCharacteristicFilter = c => c.SequenceId == subsequences[j].Id && c.CharacteristicTypeLinkId == secondCharacteristicTypeLinkId;

                        if (subsequenceDbCharacteristics.Any(secondCharacteristicFilter))
                        {
                            subsequenceCharacteristicValue = subsequenceDbCharacteristics.Single(secondCharacteristicFilter).Value;
                        }
                        else
                        {
                            subsequenceCharacteristicValue = secondCalculator.Calculate(libiadaSubsequences[j], secondLink);
                            var currentCharacteristic = new Characteristic
                                                            {
                                                                SequenceId = subsequences[j].Id,
                                                                CharacteristicTypeLinkId = secondCharacteristicTypeLinkId,
                                                                Value = subsequenceCharacteristicValue
                                                            };

                            newCharacteristics.Add(currentCharacteristic);
                            subsequenceDbCharacteristics.Add(currentCharacteristic);
                        }

                        var attributes = sequenceAttributeRepository.ConvertAttributesToString(subsequencesDbAttributes[subsequences[j].Id]);
                        var geneCharacteristic = new SubsequenceData(subsequences[j], subsequenceCharacteristicValue, attributes);
                        subsequencesCharacteristics.Add(geneCharacteristic);
                    }


                    subsequencesCharacteristics = subsequencesCharacteristics.OrderBy(g => g.Characteristic).ToList();
                    var webApiId = db.DnaSequence.Single(c => c.MatterId == matterId && c.NotationId == Aliases.Notation.Nucleotide).WebApiId;
                    sequenceCharacteristics.Add(new SequenceData(matterId, matterName, webApiId, sequenceCharacteristicValue, subsequencesCharacteristics));
                }

                // trying to save calculated characteristics to database
                characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

                sequenceCharacteristics = sequenceCharacteristics.OrderBy(r => r.Characteristic).ToList();

                var sequenceCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(firstCharacteristicTypeLinkId, Aliases.Notation.Nucleotide);
                var subsequencesCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(secondCharacteristicTypeLinkId, Aliases.Notation.Nucleotide);

                var features = featureRepository.GetFeaturesById(featureIds);
                var featuresSelectList = features.Select(f => new { Value = f.Id, Text = f.Name, Selected = true });

                var resultData = new Dictionary<string, object>
                                 {
                                     { "result", sequenceCharacteristics },
                                     { "subsequencesCharacteristicName", subsequencesCharacteristicName },
                                     { "sequenceCharacteristicName", sequenceCharacteristicName },
                                     { "features", featuresSelectList }
                                 };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(resultData) }
                };
            });
        }
    }
}
