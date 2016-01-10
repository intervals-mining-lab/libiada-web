namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    /// <summary>
    /// The subsequences calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

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
        /// Initializes a new instance of the <see cref="SubsequencesCalculationController"/> class.
        /// </summary>
        public SubsequencesCalculationController()
            : base("Subsequences characteristics calculation")
        {
            db = new LibiadaWebEntities();
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
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, int[] characteristicTypeLinkIds, int[] featureIds)
        {
            return Action(() =>
            {
                var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                        .Where(s => s.NotationId == Aliases.Notation.Nucleotide && matterIds.Contains(s.MatterId))
                                        .Select(s => new { s.Id, MatterName = s.Matter.Name })
                                        .ToDictionary(s => s.Id);
                var parentSequenceIds = parentSequences.Keys.ToArray();

                var characteristics = new List<SubsequenceData>[parentSequenceIds.Length];
                var matterNames = new string[parentSequenceIds.Length];
                var characteristicNames = new string[characteristicTypeLinkIds.Length];
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k]);
                }

                // cycle through matters; first level of characteristics array
                for (int m = 0; m < parentSequenceIds.Length; m++)
                {
                    var newCharacteristics = new List<Characteristic>();
                    var parentSequenceId = parentSequenceIds[m];
                    var dbSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceId, featureIds);
                    var subsequenceIds = dbSubsequences.Select(s => s.Id).ToArray();
                    var dbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);
                    var dbCharacteristics = db.Characteristic
                                          .Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && subsequenceIds.Contains(c.SequenceId))
                                          .ToArray()
                                          .GroupBy(c => c.SequenceId)
                                          .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

                    var sequences = subsequenceExtractor.ExtractChains(dbSubsequences, parentSequenceId);
                    matterNames[m] = parentSequences[parentSequenceId].MatterName;
                    characteristics[m] = new List<SubsequenceData>();

                    for (int j = 0; j < sequences.Length; j++)
                    {
                        var values = new double[characteristicTypeLinkIds.Length];
                        Dictionary<int, double> sequenceDbCharacteristics;
                        if (!dbCharacteristics.TryGetValue(dbSubsequences[j].Id, out sequenceDbCharacteristics))
                        {
                            sequenceDbCharacteristics = new Dictionary<int, double>();
                        }

                        // cycle through characteristics and notations; second level of characteristics array
                        for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                        {
                            int characteristicTypeLinkId = characteristicTypeLinkIds[i];

                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                            if (!sequenceDbCharacteristics.TryGetValue(characteristicTypeLinkId, out values[i]))
                            {
                                values[i] = calculator.Calculate(sequences[j], link);
                                var currentCharacteristic = new Characteristic
                                {
                                    SequenceId = dbSubsequences[j].Id,
                                    CharacteristicTypeLinkId = characteristicTypeLinkId,
                                    Value = values[i]
                                };

                                newCharacteristics.Add(currentCharacteristic);
                            }
                        }

                        string[] attributes;
                        if (!dbSubsequencesAttributes.TryGetValue(dbSubsequences[j].Id, out attributes))
                        {
                            attributes = new string[0];
                        }

                        characteristics[m].Add(new SubsequenceData(dbSubsequences[j], values, attributes));
                    }

                    // trying to save calculated characteristics to database
                    characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "matterNames", matterNames },
                    { "characteristicNames", characteristicNames },
                    { "features", featureRepository.Features.ToDictionary(f => f.Id, f => f.Name) }
                };
            });
        }

        /// <summary>
        /// The sort key value pair list.
        /// </summary>
        /// <param name="arrayForSort">
        /// The array for sort.
        /// </param>
        private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
        {
            arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }
    }
}
