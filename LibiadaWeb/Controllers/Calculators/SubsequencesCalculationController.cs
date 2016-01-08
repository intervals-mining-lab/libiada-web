namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
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
        /// <param name="sort">
        /// The is sort.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, int[] characteristicTypeLinkIds, int[] featureIds, bool sort)
        {
            return Action(() =>
            {
                var characteristics = new List<List<List<KeyValuePair<int, double>>>>();
                var matterNames = new List<string>();
                var sequenceAttributes = new List<List<List<string>>>();
                var sequencesPositions = new List<List<long>>();
                var sequenceFeatures = new List<List<string>>();
                var characteristicNames = new List<string>();
                var newCharacteristics = new List<Characteristic>();

                var parentSequences = db.DnaSequence.Where(s => s.NotationId == Aliases.Notation.Nucleotide && matterIds.Contains(s.MatterId))
                                        .ToDictionary(s => s.Id);
                var parentSequenceIds = parentSequences.Keys.ToArray();
                var allSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceIds, featureIds);
                var subsequenceIds = allSubsequences.SelectMany(s => s).Select(s => s.Id);
                Func<Characteristic, bool> characteristicsFilter = c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) 
                                                                     && subsequenceIds.Contains(c.SequenceId);
                var dbCharacteristics = db.Characteristic.Where(characteristicsFilter).ToList();
                var dbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);

                // cycle through matters; first level of characteristics array
                foreach (long parentSequenceId in parentSequenceIds)
                {
                    var matterId = parentSequences[parentSequenceId].MatterId;
                    matterNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    sequenceAttributes.Add(new List<List<string>>());
                    sequencesPositions.Add(new List<long>());
                    sequenceFeatures.Add(new List<string>());
                    characteristics.Add(new List<List<KeyValuePair<int, double>>>());

                    List<Subsequence> subsequences = allSubsequences[parentSequenceId].ToList();
                    var sequences = subsequenceExtractor.ExtractChains(subsequences, parentSequenceId);

                    foreach (Subsequence subsequence in subsequences)
                    {
                        var attributes = sequenceAttributeRepository.ConvertAttributesToString(dbSubsequencesAttributes[subsequence.Id]);
                        sequenceAttributes.Last().Add(attributes);
                        sequencesPositions.Last().Add(subsequence.Start);
                        sequenceFeatures.Last().Add(featureRepository.GetFeatureById(subsequence.FeatureId).Name);
                    }

                    // cycle through characteristics and notations; second level of characteristics array
                    for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                    {
                        characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                        int characteristicTypeLinkId = characteristicTypeLinkIds[i];

                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                        IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                        var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                        for (int j = 0; j < sequences.Count; j++)
                        {
                            double value;

                            Func<Characteristic, bool> characteristicFilter = c => c.SequenceId == subsequences[j].Id && c.CharacteristicTypeLinkId == characteristicTypeLinkId;

                            if (dbCharacteristics.Any(characteristicFilter))
                            {
                                value = dbCharacteristics.Single(characteristicFilter).Value;
                            }
                            else
                            {
                                value = calculator.Calculate(sequences[j], link);
                                var currentCharacteristic = new Characteristic
                                {
                                    SequenceId = subsequences[j].Id,
                                    CharacteristicTypeLinkId = characteristicTypeLinkId,
                                    Value = value
                                };

                                newCharacteristics.Add(currentCharacteristic);
                            }

                            characteristics.Last().Last().Add(new KeyValuePair<int, double>(j, value));
                        }
                    }
                }

                // trying to save calculated characteristics to database
                characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

                // characteristics names
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k]));
                }

                // rank sorting
                if (sort)
                {
                    for (int f = 0; f < matterIds.Length; f++)
                    {
                        for (int p = 0; p < characteristics[f].Count; p++)
                        {
                            SortKeyValuePairList(characteristics[f][p]);
                        }
                    }
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = characteristicNames[i],
                        Selected = false
                    });
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "matterNames", matterNames },
                    { "sequenceAttributes", sequenceAttributes },
                    { "sequencesPositions", sequencesPositions },
                    { "sequenceFeatures", sequenceFeatures },
                    { "characteristicNames", characteristicNames },
                    { "matterIds", matterIds },
                    { "characteristicsList", characteristicsList }
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
