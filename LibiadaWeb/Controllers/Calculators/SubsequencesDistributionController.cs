namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
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
        public SubsequencesDistributionController() : base("Subsequences distribution")
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
                var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                        .Where(s => s.NotationId == Aliases.Notation.Nucleotide && matterIds.Contains(s.MatterId))
                                        .Select(s => new { s.Id, MatterName = s.Matter.Name, s.MatterId, s.WebApiId })
                                        .ToDictionary(s => s.Id);
                var parentSequenceIds = parentSequences.Keys.ToArray();
                
                var parentDbCharacteristics = db.Characteristic
                                                .Where(c => c.CharacteristicTypeLinkId == firstCharacteristicTypeLinkId && parentSequenceIds.Contains(c.SequenceId))
                                                .ToArray()
                                                .GroupBy(c => c.SequenceId)
                                                .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));
                
                var sequenceData = new SequenceData[parentSequenceIds.Length];

                for (int i = 0; i < parentSequenceIds.Length; i++)
                {
                    // complete sequence calculations
                    var newCharacteristics = new List<Characteristic>();
                    var parentSequenceId = parentSequenceIds[i];
                    double sequenceCharacteristicValue;
                    Dictionary<int, double> sequenceDbCharacteristics;
                    if (!parentDbCharacteristics.TryGetValue(parentSequenceId, out sequenceDbCharacteristics))
                    {
                        sequenceDbCharacteristics = new Dictionary<int, double>();
                    }
                    
                    if (!sequenceDbCharacteristics.TryGetValue(firstCharacteristicTypeLinkId, out sequenceCharacteristicValue))
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
                    }

                    // all subsequence calculations
                    var dbSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceId, featureIds);
                    var subsequenceIds = dbSubsequences.Select(s => s.Id).ToArray();
                    var subsequencesData = new SubsequenceData[dbSubsequences.Length];
                    var subsequencesDbAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);
                    var subsequencesDbCharacteristics = db.Characteristic
                                                         .Where(c => c.CharacteristicTypeLinkId == secondCharacteristicTypeLinkId && subsequenceIds.Contains(c.SequenceId))
                                                         .ToArray()
                                                         .GroupBy(c => c.SequenceId)
                                                         .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

                    var libiadaSubsequences = subsequenceExtractor.ExtractChains(dbSubsequences, parentSequenceId);
                    string secondClassName = characteristicTypeLinkRepository.GetCharacteristicType(secondCharacteristicTypeLinkId).ClassName;
                    IFullCalculator secondCalculator = CalculatorsFactory.CreateFullCalculator(secondClassName);
                    var secondLink = characteristicTypeLinkRepository.GetLibiadaLink(secondCharacteristicTypeLinkId);

                    for (int j = 0; j < dbSubsequences.Length; j++)
                    {
                        double subsequenceCharacteristicValue;
                        Dictionary<int, double> subsequenceDbCharacteristics;
                        if (!subsequencesDbCharacteristics.TryGetValue(dbSubsequences[j].Id, out subsequenceDbCharacteristics))
                        {
                            subsequenceDbCharacteristics = new Dictionary<int, double>();
                        }

                        if (!subsequenceDbCharacteristics.TryGetValue(secondCharacteristicTypeLinkId, out subsequenceCharacteristicValue))
                        {
                            subsequenceCharacteristicValue = secondCalculator.Calculate(libiadaSubsequences[j], secondLink);
                            var currentCharacteristic = new Characteristic
                                                            {
                                                                SequenceId = dbSubsequences[j].Id,
                                                                CharacteristicTypeLinkId = secondCharacteristicTypeLinkId,
                                                                Value = subsequenceCharacteristicValue
                                                            };

                            newCharacteristics.Add(currentCharacteristic);
                        }

                        string[] attributes;
                        if (!subsequencesDbAttributes.TryGetValue(dbSubsequences[j].Id, out attributes))
                        {
                            attributes = new string[0];
                        }

                        subsequencesData[j] = new SubsequenceData(dbSubsequences[j], new[] { subsequenceCharacteristicValue }, attributes);
                    }

                    var parent = parentSequences[parentSequenceId];
                    sequenceData[i] = new SequenceData(parent.MatterId, parent.MatterName, parent.WebApiId, sequenceCharacteristicValue, subsequencesData);
                    
                    // trying to save calculated characteristics to database
                    characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
                }

                // sorting organisms by their characteristic
                sequenceData = sequenceData.OrderBy(r => r.Characteristic).ToArray();

                var sequenceCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(firstCharacteristicTypeLinkId, Aliases.Notation.Nucleotide);
                var subsequencesCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(secondCharacteristicTypeLinkId, Aliases.Notation.Nucleotide);

                var features = featureRepository.GetFeaturesById(featureIds);
                var featuresSelectList = features.Select(f => new { Value = f.Id, Text = f.Name, Selected = true });

                var resultData = new Dictionary<string, object>
                                 {
                                     { "result", sequenceData },
                                     { "subsequencesCharacteristicName", subsequencesCharacteristicName },
                                     { "sequenceCharacteristicName", sequenceCharacteristicName },
                                     { "features", featuresSelectList.ToDictionary(f => f.Value) },
                                     { "featuresNames", featureRepository.Features.ToDictionary(f => f.Id, f => f.Name) }
                                 };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(resultData) }
                };
            });
        }
    }
}
