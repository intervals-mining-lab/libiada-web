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
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The congeneric calculation controller.
    /// </summary>
    public class CongenericCalculationController : AbstractCalculationController
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
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CongenericCalculationController"/> class.
        /// </summary>
        public CongenericCalculationController()
        {
            db = new LibiadaWebEntities();
            this.matterRepository = new MatterRepository(db);
            this.characteristicRepository = new CharacteristicTypeRepository(db);
            this.notationRepository = new NotationRepository(db);
            this.chainRepository = new ChainRepository(db);
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
            List<matter> matters = db.matter.Include("nature").ToList();
            ViewBag.matterCheckBoxes = this.matterRepository.GetSelectListItems(matters, null);
            ViewBag.matters = matters;

            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.congeneric_chain_applicable);

            var characteristicTypes = this.characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var links = new SelectList(db.link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Нет" });

            var translators = new SelectList(db.translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", this.matterRepository.GetSelectListWithNature() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", this.notationRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(db.nature, "id", "name") }, 
                    { "links", links }, 
                    { "languages", new SelectList(db.language, "id", "name") }, 
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
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                elementNames.Add(new List<string>());
                characteristics.Add(new List<List<KeyValuePair<int, double>>>());
                theoreticalRanks.Add(new List<List<double>>());

                // Перебор всех характеристик и форм записи; второй уровень массива характеристик
                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    int notationId = notationIds[i];
                    
                    long chainId;

                    if (db.matter.Single(m => m.id == matterId).nature_id == Aliases.NatureLiterature)
                    {
                        int languageId = languageIds[i];
                        int? translatorId = translatorIds[i];

                        isLiteratureChain = true;
                        chainId = db.literature_chain.Single(l => l.matter_id == matterId &&
                                    l.notation_id == notationId
                                    && l.language_id == languageId
                                    && ((translatorId == null && l.translator_id == null) || (translatorId == l.translator_id))).id;
                    }
                    else
                    {
                        chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                    }

                    Chain libiadaChain = this.chainRepository.ToLibiadaChain(chainId);
                    libiadaChain.FillIntervalManagers();
                    characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                    int characteristicId = characteristicIds[i];
                    int? linkId = linkIds[i];

                    string className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                    ICongenericCalculator calculator = CalculatorsFactory.CreateCongenericCalculator(className);
                    var link = linkId != null ? (Link)db.link.Single(l => l.id == linkId).id : Link.None;
                    List<long> chainElements = this.chainRepository.GetElementIds(chainId);
                    int calculated = db.congeneric_characteristic.Count(c => c.chain_id == chainId &&
                                                                              c.characteristic_type_id == characteristicId &&
                                                                              ((linkId == null && c.link_id == null) || (linkId == c.link_id)));
                    if (calculated < libiadaChain.Alphabet.Cardinality)
                    {
                        for (int j = 0; j < libiadaChain.Alphabet.Cardinality; j++)
                        {
                            long elementId = chainElements[j];

                            CongenericChain tempChain = libiadaChain.CongenericChain(j);

                            if (!db.congeneric_characteristic.Any(b =>
                                                                   b.chain_id == chainId &&
                                                                   b.characteristic_type_id == characteristicId &&
                                                                   b.element_id == elementId && b.link_id == linkId))
                            {
                                double value = calculator.Calculate(tempChain, link);
                                var currentCharacteristic = new congeneric_characteristic
                                {
                                    chain_id = chainId, 
                                    characteristic_type_id = characteristicId, 
                                    link_id = linkId, 
                                    element_id = elementId, 
                                    value = value, 
                                    value_string = value.ToString(), 
                                };

                                db.congeneric_characteristic.Add(currentCharacteristic);
                                db.SaveChanges();
                            }
                        }
                    }

                    // Перебор всех элементов алфавита; третий уровень массива характеристик
                    for (int d = 0; d < libiadaChain.Alphabet.Cardinality; d++)
                    {
                        long elementId = chainElements[d];

                        double? characteristic = db.congeneric_characteristic.Single(c =>
                                    c.chain_id == chainId &&
                                    c.characteristic_type_id == characteristicId &&
                                    c.element_id == elementId &&
                                    ((linkId == null && c.link_id == null) || (linkId == c.link_id))).value;

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
                        ICongenericCalculator countCalculator = CalculatorsFactory.CreateCongenericCalculator("Count");
                        var counts = new List<int>();
                        for (int f = 0; f < libiadaChain.Alphabet.Cardinality; f++)
                        {
                            counts.Add((int)countCalculator.Calculate(libiadaChain.CongenericChain(f), Link.End));
                        }

                        ICongenericCalculator frequencyCalculator = CalculatorsFactory.CreateCongenericCalculator("Probability");
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
                
                string characteristicType = db.characteristic_type.Single(c => c.id == characteristicId).name;
                if (isLiteratureChain)
                {
                    int languageId = languageIds[k];
                    string language = db.language.Single(l => l.id == languageId).name;
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
                        this.SortKeyValuePairList(characteristics[f][p]);
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

            this.TempData["result"] = new Dictionary<string, object>
                                     {
                                         { "characteristics", characteristics }, 
                                         { "chainNames", chainNames }, 
                                         { "elementNames", elementNames }, 
                                         { "characteristicNames", characteristicNames }, 
                                         { "matterIds", matterIds }, 
                                         { "theoreticalRanks", theoreticalRanks },
                                         { "characteristicsList", characteristicsList }
                                     };

            return this.RedirectToAction("Result");
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