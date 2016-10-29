namespace LibiadaWeb.Controllers.Calculators
{
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
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.GetSubsequencesViewData(1, int.MaxValue, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicTypeLinkId">
        /// Full sequence characteristic type and link id.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// Subsequences characteristics types and links ids.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, int characteristicTypeLinkId, int[] characteristicTypeLinkIds, int[] featureIds)
        {
            return Action(() =>
            {
                var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                        .Where(s => s.NotationId == Aliases.Notation.Nucleotide && matterIds.Contains(s.MatterId))
                                        .Select(s => new { s.Id, MatterName = s.Matter.Name, s.MatterId, s.RemoteId })
                                        .ToDictionary(s => s.Id);
                var parentSequenceIds = parentSequences.Keys.ToArray();

                var parentDbCharacteristics = db.Characteristic
                                                .Where(c => c.CharacteristicTypeLinkId == characteristicTypeLinkId && parentSequenceIds.Contains(c.SequenceId))
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

                    if (!sequenceDbCharacteristics.TryGetValue(characteristicTypeLinkId, out sequenceCharacteristicValue))
                    {
                        Chain sequence = commonSequenceRepository.ToLibiadaChain(parentSequenceId);
                        sequence.FillIntervalManagers();
                        string firstClassName = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                        IFullCalculator firstCalculator = CalculatorsFactory.CreateFullCalculator(firstClassName);
                        var firstLink = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                        sequenceCharacteristicValue = firstCalculator.Calculate(sequence, firstLink);

                        var dataBaseCharacteristic = new Characteristic
                                                         {
                                                             SequenceId = parentSequenceId,
                                                             CharacteristicTypeLinkId = characteristicTypeLinkId,
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
                                                         .Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && subsequenceIds.Contains(c.SequenceId))
                                                         .ToArray()
                                                         .GroupBy(c => c.SequenceId)
                                                         .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

                    var libiadaSubsequences = subsequenceExtractor.ExtractChains(dbSubsequences, parentSequenceId);

                    var subsequenceCalculators = new IFullCalculator[characteristicTypeLinkIds.Length];
                    var subsequenceLinks = new Link[characteristicTypeLinkIds.Length];
                    for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                    {
                        subsequenceLinks[k] = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[k]);
                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[k]).ClassName;
                        subsequenceCalculators[k] = CalculatorsFactory.CreateFullCalculator(className);
                    }

                    for (int j = 0; j < dbSubsequences.Length; j++)
                    {
                        Dictionary<int, double> subsequenceDbCharacteristics;
                        if (!subsequencesDbCharacteristics.TryGetValue(dbSubsequences[j].Id, out subsequenceDbCharacteristics))
                        {
                            subsequenceDbCharacteristics = new Dictionary<int, double>();
                        }

                        double[] subsequenceCharacteristicsValue = new double[characteristicTypeLinkIds.Length];
                        for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                        {
                            if (!subsequenceDbCharacteristics.TryGetValue(characteristicTypeLinkIds[k], out subsequenceCharacteristicsValue[k]))
                            {
                                subsequenceCharacteristicsValue[k] = subsequenceCalculators[k].Calculate(libiadaSubsequences[j], subsequenceLinks[k]);
                                var currentCharacteristic = new Characteristic
                                {
                                    SequenceId = dbSubsequences[j].Id,
                                    CharacteristicTypeLinkId = characteristicTypeLinkIds[k],
                                    Value = subsequenceCharacteristicsValue[k]
                                };

                                newCharacteristics.Add(currentCharacteristic);
                            }
                        }

                        Dictionary<string, string> attributes;
                        if (!subsequencesDbAttributes.TryGetValue(dbSubsequences[j].Id, out attributes))
                        {
                            attributes = new Dictionary<string, string>();
                        }

                        subsequencesData[j] = new SubsequenceData(dbSubsequences[j], subsequenceCharacteristicsValue, attributes);
                    }

                    var parent = parentSequences[parentSequenceId];
                    subsequencesData = subsequencesData.OrderByDescending(s => s.CharacteristicsValues[0]).ToArray();
                    sequenceData[i] = new SequenceData(parent.MatterId, parent.MatterName, parent.RemoteId, sequenceCharacteristicValue, subsequencesData);

                    // trying to save calculated characteristics to database
                    characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
                }

                // sorting organisms by their characteristic
                sequenceData = sequenceData.OrderBy(r => r.Characteristic).ToArray();

                var sequenceCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId, Aliases.Notation.Nucleotide);

                var subsequencesCharacteristicsNames = new string[characteristicTypeLinkIds.Length];
                var subsequencesCharacteristicsList = new SelectListItem[characteristicTypeLinkIds.Length];
                for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                {
                    subsequencesCharacteristicsNames[i] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[i], Aliases.Notation.Nucleotide);
                    subsequencesCharacteristicsList[i] = new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = subsequencesCharacteristicsNames[i],
                        Selected = false
                    };
                }

                var features = featureRepository.GetFeaturesById(featureIds);
                var featuresSelectList = features.Select(f => new { Value = f.Id, Text = f.Name, Selected = true });

                var resultData = new Dictionary<string, object>
                                 {
                                     { "result", sequenceData },
                                     { "subsequencesCharacteristicsNames", subsequencesCharacteristicsNames },
                                     { "subsequencesCharacteristicsList", subsequencesCharacteristicsList },
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
