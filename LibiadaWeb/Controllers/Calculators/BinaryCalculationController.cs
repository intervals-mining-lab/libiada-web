using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.BinaryCalculators;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Calculators
{
    public class BinaryCalculationController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly NotationRepository notationRepository;
        private readonly ChainRepository chainRepository;

        public BinaryCalculationController()
        {
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            notationRepository = new NotationRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            ViewBag.matters = db.matter.ToList();
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            IEnumerable<characteristic_type> characteristics =
                db.characteristic_type.Where(c => new List<int>{3,5,6,7}.Contains(c.characteristic_applicability_id));
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristics, null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long matterId, int characteristicId, int linkUpId, int notationId, int languageId,
                                  int filterSize, bool filter)
        {
            List<binary_characteristic> characteristics = new List<binary_characteristic>();
            List<element> elements = new List<element>();
            List<binary_characteristic> filteredResult = null;
            List<element> firstElements = new List<element>();
            List<element> secondElements = new List<element>();

            chain dbChain;
            if (db.matter.Single(m => m.id == matterId).nature_id == 3)
            {
                long chainId =
                    db.literature_chain.Single(
                        l => l.matter_id == matterId && l.notation_id == notationId && l.language_id == languageId).id;
                dbChain = db.chain.Single(c => c.id == chainId);
            }
            else
            {
                dbChain = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId);
            }

            Chain currentChain = chainRepository.FromDbChainToLibiadaChain(dbChain.id);
            String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;

            IBinaryCharacteristicCalculator calculator = BinaryCharacteristicsFactory.Create(className);
            LinkUp linkUp = (LinkUp) linkUpId;
            int calculated = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                 b.characteristic_type_id == characteristicId &&
                                                                 b.link_up_id == linkUpId).Count();
            if (calculated < currentChain.Length)
            {
                for (int i = 0; i < currentChain.Alphabet.Power; i++)
                {
                    for (int j = 0; j < currentChain.Alphabet.Power; j++)
                    {
                        long firstElementId = dbChain.alphabet.Single(a => a.number == i + 1).element_id;
                        long secondElementId = dbChain.alphabet.Single(a => a.number == j + 1).element_id;
                        if (!db.binary_characteristic.Any(b =>
                                                         b.chain_id == dbChain.id &&
                                                         b.characteristic_type_id == characteristicId &&
                                                         b.first_element_id == firstElementId &&
                                                         b.second_element_id == secondElementId &&
                                                         b.link_up_id == linkUpId))
                        {
                            binary_characteristic currentCharacteristic = new binary_characteristic();
                            currentCharacteristic.id =
                                db.ExecuteStoreQuery<long>("SELECT seq_next_value('characteristics_id_seq')").First();
                            currentCharacteristic.chain_id = dbChain.id;
                            currentCharacteristic.characteristic_type_id = characteristicId;
                            currentCharacteristic.link_up_id = linkUpId;
                            currentCharacteristic.first_element_id = firstElementId;
                            currentCharacteristic.second_element_id = secondElementId;
                            currentCharacteristic.value = calculator.Calculate(currentChain, 
                                currentChain.Alphabet[i], currentChain.Alphabet[j], linkUp);
                            currentCharacteristic.value_string = currentCharacteristic.value.ToString();
                            currentCharacteristic.creation_date = DateTime.Now;
                            db.binary_characteristic.AddObject(currentCharacteristic);
                            db.SaveChanges();
                        }
                    }
                }
            }


            if (filter)
            {
                filteredResult = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                     b.characteristic_type_id == characteristicId &&
                                                                     b.link_up_id == linkUpId)
                                   .OrderByDescending(b => b.value)
                                   .Take(filterSize).ToList();

                for (int l = 0; l < filterSize; l++)
                {
                    long firstElementId = filteredResult[l].first_element_id;
                    firstElements.Add(db.element.Single(e => e.id == firstElementId));
                }
                for (int m = 0; m < filterSize; m++)
                {
                    long secondElementId = filteredResult[m].second_element_id;
                    secondElements.Add(db.element.Single(e => e.id == secondElementId));
                }
            }
            else
            {
                characteristics = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                      b.characteristic_type_id == characteristicId &&
                                                                      b.link_up_id == linkUpId)
                                    .OrderBy(b => b.second_element_id)
                                    .ThenBy(b => b.first_element_id)
                                    .ToList();
                for (int m = 0; m < currentChain.Alphabet.Power; m++)
                {
                    long firstElementId = characteristics[m].first_element_id;
                    elements.Add(db.element.Single(e => e.id == firstElementId));
                }
            }

            TempData["filter"] = filter;
            TempData["filteredResult"] = filteredResult;
            TempData["firstElements"] = firstElements;
            TempData["secondElements"] = secondElements;
            TempData["filtersize"] = filterSize;
            TempData["characteristics"] = characteristics;
            TempData["elements"] = elements;
            TempData["characteristicName"] =
                db.characteristic_type.Single(charact => charact.id == characteristicId).name;
            TempData["chainName"] = db.matter.Single(m => m.id == matterId).name;
            TempData["notationId"] = notationId;

            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            ViewBag.chainName = TempData["chainName"] as String;
            ViewBag.characteristicName = TempData["characteristicName"] as String;
            int notationId = (int) TempData["notationId"];
            ViewBag.notationName = db.notation.Single(n => n.id == notationId).name;
            ViewBag.isFilter = TempData["filter"];

            if ((bool) TempData["filter"])
            {
                ViewBag.filtersize = TempData["filtersize"];
                ViewBag.filteredResult = TempData["filteredResult"] as List<binary_characteristic>;
                ViewBag.firstElements = TempData["firstElements"];
                ViewBag.secondElements = TempData["secondElements"];
            }
            else
            {
                ViewBag.characteristics = TempData["characteristics"] as List<binary_characteristic>;
                ViewBag.elements = TempData["elements"] as List<element>;
            }
            return View();
        }
    }
}
