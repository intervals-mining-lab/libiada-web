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
                var allSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceIds, featureIds);
                var subsequenceIds = allSubsequences.SelectMany(s => s.Value).Select(s => s.Id);
                var dbCharacteristics = db.Characteristic
                                          .Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && subsequenceIds.Contains(c.SequenceId))
                                          .ToList();
                var dbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);

                var characteristics = new List<SubsequenceData>[parentSequenceIds.Length];
                var matterNames = new string[parentSequenceIds.Length];

                var characteristicNames = new string[characteristicTypeLinkIds.Length];
                var characteristicsList = new SelectListItem[characteristicTypeLinkIds.Length];
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k]);
                    characteristicsList[k] = new SelectListItem
                    {
                        Value = k.ToString(),
                        Text = characteristicNames[k],
                        Selected = false
                    };
                }

                // cycle through matters; first level of characteristics array
                for (int m = 0; m < parentSequenceIds.Length; m++)
                {
                    var newCharacteristics = new List<Characteristic>();
                    var parentSequenceId = parentSequenceIds[m];
                    matterNames[m] = parentSequences[parentSequenceId].MatterName;
                    characteristics[m] = new List<SubsequenceData>();

                    Subsequence[] subsequences = allSubsequences[parentSequenceId].ToArray();
                    var sequences = subsequenceExtractor.ExtractChains(subsequences, parentSequenceId);
                    

                    for (int j = 0; j < sequences.Length; j++)
                    {
                        string[] attributes;
                        if (!dbSubsequencesAttributes.TryGetValue(subsequences[j].Id, out attributes))
                        {
                            attributes = new string[0];
                        }
                        
                        var values = new double[characteristicTypeLinkIds.Length];

                        // cycle through characteristics and notations; second level of characteristics array
                        for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                        {
                            int characteristicTypeLinkId = characteristicTypeLinkIds[i];

                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                            Func<dynamic, bool> characteristicFilter = c => c.SequenceId == subsequences[j].Id && c.CharacteristicTypeLinkId == characteristicTypeLinkId;

                            if (dbCharacteristics.Any(characteristicFilter))
                            {
                                var characteristic = dbCharacteristics.Single(characteristicFilter);
                                values[i] = characteristic.Value;
                                dbCharacteristics.Remove(characteristic);
                            }
                            else
                            {
                                values[i] = calculator.Calculate(sequences[j], link);
                                var currentCharacteristic = new Characteristic
                                {
                                    SequenceId = subsequences[j].Id,
                                    CharacteristicTypeLinkId = characteristicTypeLinkId,
                                    Value = values[i]
                                };

                                newCharacteristics.Add(currentCharacteristic);
                            }
                        }

                        characteristics[m].Add(new SubsequenceData(subsequences[j], values, attributes));
                    }

                    // trying to save calculated characteristics to database
                    characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "matterNames", matterNames },
                    { "characteristicNames", characteristicNames },
                    { "characteristicsList", characteristicsList },
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
