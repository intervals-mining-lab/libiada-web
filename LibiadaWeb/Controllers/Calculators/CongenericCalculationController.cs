namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using Models;
    using Models.Repositories.Catalogs;
    using Models.Repositories.Chains;

    /// <summary>
    /// The congeneric calculation controller.
    /// </summary>
    public class CongenericCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly CommonSequenceRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CongenericCalculationController"/> class.
        /// </summary>
        public CongenericCalculationController() : base("CongenericCalculation", "Congeneric calculation")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            chainRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            var matters = db.Matter.Include("nature");
            ViewBag.matterCheckBoxes = matterRepository.GetSelectListItems(matters, null);
            ViewBag.matters = matters;

            var characteristicsList = db.CharacteristicType.Where(c => c.CongenericSequenceApplicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var links = new SelectList(db.Link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetSelectListWithNature() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(db.Nature, "id", "name") }, 
                    { "links", links }, 
                    { "languages", new SelectList(db.Language, "id", "name") }, 
                    { "translators", translators }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="languageIds">
        /// The language ids.
        /// </param>
        /// <param name="translatorIds">
        /// The translator ids.
        /// </param>
        /// <param name="isSort">
        /// The is sort.
        /// </param>
        /// <param name="theoretical">
        /// The theoretical.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds, 
            int[] characteristicIds, 
            int?[] linkIds, 
            int[] notationIds, 
            int[] languageIds, 
            int?[] translatorIds, 
            bool isSort, 
            bool theoretical)
        {
            return Action(() =>
            {
                var characteristics = new List<List<List<KeyValuePair<int, double>>>>();
                var theoreticalRanks = new List<List<List<double>>>();
                var chainNames = new List<string>();
                var elementNames = new List<List<string>>();
                var characteristicNames = new List<string>();

                bool isLiteratureChain = false;

                // Перебор всех цепочек; первый уровень массива характеристик
                for (int w = 0; w < matterIds.Length; w++)
                {
                    long matterId = matterIds[w];
                    chainNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    elementNames.Add(new List<string>());
                    characteristics.Add(new List<List<KeyValuePair<int, double>>>());
                    theoreticalRanks.Add(new List<List<double>>());

                    // Перебор всех характеристик и форм записи; второй уровень массива характеристик
                    for (int i = 0; i < characteristicIds.Length; i++)
                    {
                        int notationId = notationIds[i];

                        long chainId;

                        if (db.Matter.Single(m => m.Id == matterId).NatureId == Aliases.Nature.Literature)
                        {
                            int languageId = languageIds[i];
                            int? translatorId = translatorIds[i];

                            isLiteratureChain = true;
                            chainId = db.LiteratureSequence.Single(l => l.MatterId == matterId &&
                                                                      l.NotationId == notationId
                                                                      && l.LanguageId == languageId
                                                                      &&
                                                                      ((translatorId == null && l.TranslatorId == null) ||
                                                                       (translatorId == l.TranslatorId))).Id;
                        }
                        else
                        {
                            chainId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                        }

                        Chain libiadaChain = chainRepository.ToLibiadaChain(chainId);
                        libiadaChain.FillIntervalManagers();
                        characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                        int characteristicId = characteristicIds[i];
                        int? linkId = linkIds[i];

                        string className = db.CharacteristicType.Single(c => c.Id == characteristicId).ClassName;
                        ICongenericCalculator calculator = CalculatorsFactory.CreateCongenericCalculator(className);
                        var link = (Link)(linkId ?? 0);
                        List<long> chainElements = chainRepository.GetElementIds(chainId);
                        int calculated = db.CongenericCharacteristic.Count(c => c.SequenceId == chainId &&
                                                                                 c.CharacteristicTypeId ==
                                                                                 characteristicId &&
                                                                                 ((linkId == null && c.LinkId == null) ||
                                                                                  (linkId == c.LinkId)));
                        if (calculated < libiadaChain.Alphabet.Cardinality)
                        {
                            for (int j = 0; j < libiadaChain.Alphabet.Cardinality; j++)
                            {
                                long elementId = chainElements[j];

                                CongenericChain tempChain = libiadaChain.CongenericChain(j);

                                if (!db.CongenericCharacteristic.Any(b =>
                                    b.SequenceId == chainId &&
                                    b.CharacteristicTypeId == characteristicId &&
                                    b.ElementId == elementId && b.LinkId == linkId))
                                {
                                    double value = calculator.Calculate(tempChain, link);
                                    var currentCharacteristic = new CongenericCharacteristic
                                    {
                                        SequenceId = chainId,
                                        CharacteristicTypeId = characteristicId,
                                        LinkId = linkId,
                                        ElementId = elementId,
                                        Value = value,
                                        ValueString = value.ToString(),
                                    };

                                    db.CongenericCharacteristic.Add(currentCharacteristic);
                                    db.SaveChanges();
                                }
                            }
                        }

                        // Перебор всех элементов алфавита; третий уровень массива характеристик
                        for (int d = 0; d < libiadaChain.Alphabet.Cardinality; d++)
                        {
                            long elementId = chainElements[d];

                            double? characteristic = db.CongenericCharacteristic.Single(c =>
                                c.SequenceId == chainId &&
                                c.CharacteristicTypeId == characteristicId &&
                                c.ElementId == elementId &&
                                ((linkId == null && c.LinkId == null) || (linkId == c.LinkId))).Value;

                            characteristics.Last().Last().Add(new KeyValuePair<int, double>(d, (double)characteristic));

                            if (i == 0)
                            {
                                elementNames.Last().Add(libiadaChain.Alphabet[d].ToString());
                            }
                        }

                        // теоретические частоты по критерию Орлова
                        if (theoretical)
                        {
                            theoreticalRanks[w].Add(new List<double>());
                            ICongenericCalculator countCalculator =
                                CalculatorsFactory.CreateCongenericCalculator("Count");
                            var counts = new List<int>();
                            for (int f = 0; f < libiadaChain.Alphabet.Cardinality; f++)
                            {
                                counts.Add((int)countCalculator.Calculate(libiadaChain.CongenericChain(f), Link.End));
                            }

                            ICongenericCalculator frequencyCalculator =
                                CalculatorsFactory.CreateCongenericCalculator("Probability");
                            var frequency = new List<double>();
                            for (int f = 0; f < libiadaChain.Alphabet.Cardinality; f++)
                            {
                                frequency.Add(frequencyCalculator.Calculate(libiadaChain.CongenericChain(f), Link.End));
                            }

                            double maxFrequency = frequency.Max();
                            double k = 1 / Math.Log(counts.Max());
                            double b = (k / maxFrequency) - 1;
                            int n = 1;
                            double plow = libiadaChain.GetLength();
                            double p = k / (b + n);
                            while (p >= (1 / plow))
                            {
                                theoreticalRanks.Last().Last().Add(p);
                                n++;
                                p = k / (b + n);
                            }
                        }
                    }
                }

                // подписи для характеристик
                for (int k = 0; k < characteristicIds.Length; k++)
                {
                    int characteristicId = characteristicIds[k];

                    string characteristicType = db.CharacteristicType.Single(c => c.Id == characteristicId).Name;
                    if (isLiteratureChain)
                    {
                        int languageId = languageIds[k];
                        string language = db.Language.Single(l => l.Id == languageId).Name;
                        characteristicNames.Add(characteristicType + " " + language);
                    }
                    else
                    {
                        characteristicNames.Add(characteristicType);
                    }
                }

                // ранговая сортировка
                if (isSort)
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
                    { "chainNames", chainNames },
                    { "elementNames", elementNames },
                    { "characteristicNames", characteristicNames },
                    { "matterIds", matterIds },
                    { "theoreticalRanks", theoreticalRanks },
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
