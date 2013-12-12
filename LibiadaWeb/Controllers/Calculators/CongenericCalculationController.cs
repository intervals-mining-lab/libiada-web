using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Calculators
{
    public class CongenericCalculationController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;


        public CongenericCalculationController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /Transformation/

        public ActionResult Index()
        {
            List<matter> matters = db.matter.Include("nature").ToList();
            ViewBag.matterCheckBoxes = matterRepository.GetSelectListItems(matters, null);
            ViewBag.matters = matters;

            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => Aliases.ApplicabilityCongeneric.Contains(c.characteristic_applicability_id));
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristicsList, null);

            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            ViewBag.languagesList = new SelectList(db.language, "id", "name");
            
            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] matterIds, int[] characteristicIds, int[] linkUpIds, int[] notationIds, int[] languageIds, bool isSort, bool theoretical)
        {

            List<List<List<KeyValuePair<int, double>>>> characteristics = new List<List<List<KeyValuePair<int, double>>>>();
            List<List<List<double>>> teoreticalRanks = new List<List<List<double>>>();
            List<string> chainNames = new List<string>();
            List<List<string>> elementNames = new List<List<string>>();
            List<string> characteristicNames = new List<string>();

            bool isLiteratureChain = false;

            //Перебор всех цепочек; первый уровень массива характеристик
            for (int w = 0; w < matterIds.Length; w++)
            {
                long matterId = matterIds[w];
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                elementNames.Add(new List<string>());
                characteristics.Add(new List<List<KeyValuePair<int, double>>>());
                teoreticalRanks.Add(new List<List<double>>());

                //Перебор всех характеристик и форм записи; второй уровень массива характеристик
                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    int notationId = notationIds[i];
                    int languageId = languageIds[i];
                    long chainId;
                    if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                    {
                        isLiteratureChain = true;
                        chainId =
                            db.literature_chain.Single(l => l.matter_id == matterId && 
                                    l.notation_id == notationId 
                                    && l.language_id == languageId).id;
                    }
                    else
                    {
                        chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                    }

                    Chain libiadaChain = chainRepository.ToLibiadaChain(chainId);

                    characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                    int characteristicId = characteristicIds[i];
                    int linkUpId = linkUpIds[i];

                    String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                    ICalculator calculator = CalculatorsFactory.Create(className);
                    LinkUp linkUp = (LinkUp) db.link_up.Single(l => l.id == linkUpId).id;
                    List<long> chainElements = chainRepository.GetElementIds(chainId);
                    int calculated = db.congeneric_characteristic.Count(b => b.chain_id == chainId &&
                                                                              b.characteristic_type_id == characteristicId &&
                                                                              b.link_up_id == linkUpId);
                    if (calculated < libiadaChain.Alphabet.Power)
                    {
                        
                        for (int j = 0; j < libiadaChain.Alphabet.Power; j++)
                        {
                            long elementId = chainElements[j];

                            CongenericChain tempChain = libiadaChain.CongenericChain(j);

                            if (!db.congeneric_characteristic.Any(b =>
                                                                   b.chain_id == chainId &&
                                                                   b.characteristic_type_id == characteristicId &&
                                                                   b.element_id == elementId && b.link_up_id == linkUpId))
                            {
                                double value = calculator.Calculate(tempChain, linkUp);
                                congeneric_characteristic currentCharacteristic = new congeneric_characteristic
                                    {
                                        chain_id = chainId,
                                        characteristic_type_id = characteristicId,
                                        link_up_id = linkUpId,
                                        element_id = elementId,
                                        value = value,
                                        value_string = value.ToString(),
                                        creation_date = DateTime.Now
                                    };
                                db.congeneric_characteristic.AddObject(currentCharacteristic);
                                db.SaveChanges();
                            }
                        }
                    }

                    //Перебор всех элементов алфавита; третий уровень массива характеристик
                    for (int d = 0; d < libiadaChain.Alphabet.Power; d++)
                    {
                        long elementId = chainElements[d];

                        double? characteristic = db.congeneric_characteristic.Single(b =>
                                    b.chain_id == chainId &&
                                    b.characteristic_type_id == characteristicId &&
                                    b.element_id == elementId &&
                                    b.link_up_id == linkUpId).value;
                        
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
                        List<int> counts = new List<int>();
                        for (int f = 0; f < libiadaChain.Alphabet.Power; f++)
                        {
                            counts.Add((int)countCalculator.Calculate(libiadaChain.CongenericChain(f), LinkUp.End));
                        }

                        ICalculator frequencyCalculator = CalculatorsFactory.Create("Probability");
                        List<double> frequency = new List<double>();
                        for (int f = 0; f < libiadaChain.Alphabet.Power; f++)
                        {
                            frequency.Add(frequencyCalculator.Calculate(libiadaChain.CongenericChain(f), LinkUp.End));
                        }

                        double maxFrequency = frequency.Max();
                        double K = 1/Math.Log(counts.Max());
                        double B = (K/maxFrequency) - 1;
                        int n = 1;
                        double Plow = libiadaChain.Length;
                        double P = K/(B + n);
                        while (P >= (1/Plow))
                        {
                            teoreticalRanks.Last().Last().Add(P);
                            n++;
                            P = K/(B + n);
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

            //ранговая сортировка
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

            TempData["characteristics"] = characteristics;
            TempData["chainNames"] = chainNames;
            TempData["elementNames"] = elementNames;
            TempData["characteristicNames"] = characteristicNames;
            TempData["matterIds"] = matterIds;
            TempData["teoreticalRanks"] = teoreticalRanks;
            return RedirectToAction("Result");
        }

        private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
        {
            arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }

        public ActionResult Result()
        {
            List<List<List<KeyValuePair<int, double>>>> characteristics = TempData["characteristics"] as List<List<List<KeyValuePair<int, double>>>>;
            List<String> characteristicNames = TempData["characteristicNames"] as List<String>;
            List<SelectListItem> characteristicsList = new List<SelectListItem>();
            for (int i = 0; i < characteristicNames.Count; i++)
            {
                characteristicsList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = characteristicNames[i],
                    Selected = false
                });
            }
            ViewBag.matterIds = TempData["matterIds"];
            ViewBag.characteristicsList = characteristicsList;
            ViewBag.characteristics = characteristics;
            ViewBag.chainNames = TempData["chainNames"];
            ViewBag.elementNames = TempData["elementNames"];
            ViewBag.characteristicNames = characteristicNames;
            ViewBag.theoreticalRanks = TempData["teoreticalRanks"];

            TempData.Keep();

            return View();
        }

    }
}
