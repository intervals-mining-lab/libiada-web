namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The genes calculation controller.
    /// </summary>
    public class GenesCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The gene repository.
        /// </summary>
        private readonly SubsequenceRepository subsequenceRepository;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesCalculationController"/> class.
        /// </summary>
        public GenesCalculationController() : base("GenesCalculation", "Genes calculation")
        {
            db = new LibiadaWebEntities();
            subsequenceRepository = new SubsequenceRepository(db);
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
            var calculatorsHelper = new ViewDataHelper(db);
            var data = calculatorsHelper.GetGenesCalculationData(1, int.MaxValue, true);
            ViewBag.data = data;
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
        /// <param name="notationIds">
        /// The notation ids.
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
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicTypeLinkIds,
            int[] notationIds,
            int[] featureIds,
            bool sort)
        {
            return Action(() =>
            {
                var characteristics = new List<List<List<KeyValuePair<int, double>>>>();
                var matterNames = new List<string>();
                var sequenceProducts = new List<List<string>>();
                var sequencesPositions = new List<List<long>>();
                var sequenceFeatures = new List<List<string>>();
                var characteristicNames = new List<string>();

                // Перебор всех цепочек; первый уровень массива характеристик
                for (int w = 0; w < matterIds.Length; w++)
                {
                    long matterId = matterIds[w];
                    matterNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    sequenceProducts.Add(new List<string>());
                    sequencesPositions.Add(new List<long>());
                    sequenceFeatures.Add(new List<string>());
                    characteristics.Add(new List<List<KeyValuePair<int, double>>>());

                    List<Subsequence> subsequences;
                    var chains = subsequenceRepository.ExtractSequences(matterId, notationIds[w], featureIds, out subsequences);

                    // Перебор всех характеристик и форм записи; второй уровень массива характеристик
                    for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                    {
                        characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                        int characteristicTypeLinkId = characteristicTypeLinkIds[i];

                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                        IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                        var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                        for (int j = 0; j < chains.Count; j++)
                        {
                            long subsequenceId = subsequences[j].Id;

                            if (!db.Characteristic.Any(c => c.SequenceId == subsequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId))
                            {
                                double value = calculator.Calculate(chains[j], link);
                                var currentCharacteristic = new Characteristic
                                {
                                    SequenceId = subsequenceId,
                                    CharacteristicTypeLinkId = characteristicTypeLinkId,
                                    Value = value,
                                    ValueString = value.ToString()
                                };

                                db.Characteristic.Add(currentCharacteristic);
                                db.SaveChanges();
                            }
                        }

                        for (int d = 0; d < chains.Count; d++)
                        {
                            long subsequenceId = subsequences[d].Id;
                            double characteristic = db.Characteristic.Single(c => c.SequenceId == subsequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId).Value;

                            characteristics.Last().Last().Add(new KeyValuePair<int, double>(d, characteristic));

                            if (i == 0)
                            {
                                var featureId = subsequences[d].FeatureId;
                                var sequenceId = subsequences[d].Id;

                                sequenceProducts.Last().Add(db.SequenceAttribute.Any(sa => sa.SequenceId == sequenceId && sa.AttributeId == Aliases.Attribute.Product) ? 
                                    db.SequenceAttribute.Single(sa => sa.SequenceId == sequenceId && sa.AttributeId == Aliases.Attribute.Product).Value : string.Empty);
                                sequencesPositions.Last().Add(subsequences[d].Start);

                                sequenceFeatures.Last().Add(db.Feature.Single(p => featureId == p.Id).Name);
                            }
                        }
                    }
                }

                // подписи для характеристик
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k], notationIds[k]));
                }

                // ранговая сортировка
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
                    { "sequenceProducts", sequenceProducts },
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
