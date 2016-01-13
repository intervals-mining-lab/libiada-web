namespace LibiadaWeb.Controllers.Calculators
{
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
        /// The feature repository.
        /// </summary>
        private readonly FeatureRepository featureRepository;

        /// <summary>
        /// The characteristic type link repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesCalculationController"/> class.
        /// </summary>
        public SubsequencesCalculationController() : base("Subsequences characteristics calculation")
        {
            db = new LibiadaWebEntities();
            featureRepository = new FeatureRepository(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
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

                var characteristics = new Dictionary<string, SubsequenceData[]>(parentSequenceIds.Length);
                var matterNames = new string[parentSequenceIds.Length];
                for (int n = 0; n < parentSequenceIds.Length; n++)
                {
                    matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                }
                
                var characteristicNames = new string[characteristicTypeLinkIds.Length];
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k]);
                }

                // cycle through matters; first level of characteristics array
                for (int m = 0; m < parentSequenceIds.Length; m++)
                {
                    // creating local context to avoid memory overflow due to possibly big cache of characteristics
                    using (var context = new LibiadaWebEntities())
                    {
                        var subsequenceExtractor = new SubsequenceExtractor(context);
                        var sequenceAttributeRepository = new SequenceAttributeRepository(context);
                        var newCharacteristics = new List<Characteristic>();

                        // extracting data from database
                        var dbSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceIds[m], featureIds);
                        var subsequenceIds = dbSubsequences.Select(s => s.Id).ToArray();
                        var dbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);
                        var dbCharacteristics = context.Characteristic
                                              .Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && subsequenceIds.Contains(c.SequenceId))
                                              .ToArray()
                                              .GroupBy(c => c.SequenceId)
                                              .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

                        // converting to libiada sequences
                        var sequences = subsequenceExtractor.ExtractChains(dbSubsequences, parentSequenceIds[m]);
                        characteristics.Add(matterNames[m], new SubsequenceData[sequences.Length]);
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

                            characteristics[matterNames[m]][j] = new SubsequenceData(dbSubsequences[j], values, attributes);
                        }

                        // trying to save calculated characteristics to database
                        var characteristicRepository = new CharacteristicRepository(context);
                        characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
                    }
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
