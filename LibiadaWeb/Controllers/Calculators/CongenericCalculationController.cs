namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    public class CongenericCalculationController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly ChainRepository chainRepository;

        public CongenericCalculationController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /CongenericCalculation/
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            List<matter> matters = db.matter.Include("nature").ToList();
            ViewBag.matterCheckBoxes = matterRepository.GetSelectListItems(matters, null);
            ViewBag.matters = matters;

            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.congeneric_chain_applicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var translators = new SelectList(db.translator, "id", "name").ToList();

            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetSelectListWithNature() },
                    { "characteristicTypes", characteristicTypes },
                    { "notations", notationRepository.GetSelectListWithNature() },
                    { "natures", new SelectList(db.nature, "id", "name") },
                    { "links", new SelectList(db.link, "id", "name") },
                    { "languages", new SelectList(db.language, "id", "name") },
                    { "translators", translators },
                    { "natureLiterature", Aliases.NatureLiterature }
                };
            return View();
        }

        [HttpPost]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicIds,
            int[] linkIds,
            int[] notationIds,
            int[] languageIds,
            int?[] translatorIds,
            bool isSort,
            bool theoretical)
        {
            var characteristics = new List<List<List<KeyValuePair<int, double>>>>();
            var teoreticalRanks = new List<List<List<double>>>();
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
                teoreticalRanks.Add(new List<List<double>>());

                // Перебор всех характеристик и форм записи; второй уровень массива характеристик
                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    int notationId = notationIds[i];
                    int languageId = languageIds[i];
                    int? translatorId = translatorIds[i];
                    long chainId;

                    if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                    {
                        isLiteratureChain = true;
                        chainId = db.literature_chain.Single(l => l.matter_id == matterId &&
                                    l.notation_id == notationId
                                    && l.language_id == languageId
                                    && ((translatorId == null && l.translator_id == null)
                                                    || (translatorId == l.translator_id))).id;
                    }
                    else
                    {
                        chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                    }

                    Chain libiadaChain = chainRepository.ToLibiadaChain(chainId);
                    libiadaChain.FillIntervalManagers();
                    characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                    int characteristicId = characteristicIds[i];
                    int linkId = linkIds[i];

                    string className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                    ICalculator calculator = CalculatorsFactory.Create(className);
                    Link link = (Link)db.link.Single(l => l.id == linkId).id;
                    List<long> chainElements = chainRepository.GetElementIds(chainId);
                    int calculated = db.congeneric_characteristic.Count(b => b.chain_id == chainId &&
                                                                              b.characteristic_type_id == characteristicId &&
                                                                              b.link_id == linkId);
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
                                    created = DateTime.Now
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

                        double? characteristic = db.congeneric_characteristic.Single(b =>
                                    b.chain_id == chainId &&
                                    b.characteristic_type_id == characteristicId &&
                                    b.element_id == elementId &&
                                    b.link_id == linkId).value;

                        characteristics.Last().Last().Add(
                            new KeyValuePair<int, double>(d, (double)characteristic));

                        if (i == 0)
                        {
                            elementNames.Last().Add(libiadaChain.Alphabet[d].ToString());
                        }
                    }

                    // теоретические частоты по критерию Орлова
                    if (theoretical)
                    {

                        teoreticalRanks[w].Add(new List<double>());
                        ICalculator countCalculator = CalculatorsFactory.Create("Count");
                        var counts = new List<int>();
                        for (int f = 0; f < libiadaChain.Alphabet.Cardinality; f++)
                        {
                            counts.Add((int)countCalculator.Calculate(libiadaChain.CongenericChain(f), Link.End));
                        }

                        ICalculator frequencyCalculator = CalculatorsFactory.Create("Probability");
                        var frequency = new List<double>();
                        for (int f = 0; f < libiadaChain.Alphabet.Cardinality; f++)
                        {
                            frequency.Add(frequencyCalculator.Calculate(libiadaChain.CongenericChain(f), Link.End));
                        }

                        double maxFrequency = frequency.Max();
                        double K = 1 / Math.Log(counts.Max());
                        double B = (K / maxFrequency) - 1;
                        int n = 1;
                        double Plow = libiadaChain.Length;
                        double P = K / (B + n);
                        while (P >= (1 / Plow))
                        {
                            teoreticalRanks.Last().Last().Add(P);
                            n++;
                            P = K / (B + n);
                        }
                    }
                }

            }

            // подписи для характеристик
            for (int k = 0; k < characteristicIds.Length; k++)
            {
                int characteristicId = characteristicIds[k];
                int languageId = languageIds[k];
                string characteristicType = db.characteristic_type.Single(c => c.id == characteristicId).name;
                if (isLiteratureChain)
                {
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
                        SortKeyValuePairList(characteristics[f][p]);
                    }
                }
            }

            TempData["result"] = new Dictionary<string, object>
                                     {
                                         { "characteristics", characteristics },
                                         { "chainNames", chainNames },
                                         { "elementNames", elementNames },
                                         { "characteristicNames", characteristicNames },
                                         { "matterIds", matterIds },
                                         { "teoreticalRanks", teoreticalRanks }
                                     };

            return RedirectToAction("Result");
        }

        private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
        {
            arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }

        public ActionResult Result()
        {
            try
            {
                var result = TempData["characteristics"] as Dictionary<string, object>;
                if (result == null)
                {
                    throw new Exception("Нет данных для отображения");
                }

                var characteristicNames = (List<string>)result["characteristicNames"];
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

                ViewBag.matterIds = result["matterIds"];
                ViewBag.characteristicsList = characteristicsList;
                ViewBag.characteristics = result["characteristics"];
                ViewBag.chainNames = result["chainNames"];
                ViewBag.elementNames = result["elementNames"];
                ViewBag.characteristicNames = characteristicNames;
                ViewBag.theoreticalRanks = result["teoreticalRanks"];

                TempData.Keep();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);
            }

            return View();
        }
    }
}