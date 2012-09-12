using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.BinaryCalculators;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;
using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers
{
    public class BinaryCalculationController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();
        private MatterRepository matterRepository = new MatterRepository();
        private CharacteristicTypeRepository characteristicsRepository = new CharacteristicTypeRepository();
        private LinkUpRepository linkUpRepository = new LinkUpRepository();
        private NotationRepository notationRepository = new NotationRepository();
        private ChainRepository chainRepository = new ChainRepository();

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            ViewBag.matters = db.matter.ToList();

            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.characteristicsList = characteristicsRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long matterId, int characteristicId, int linkUpId, int notationId)
        {
            List<List<Double>> characteristics = new List<List<Double>>();

            chain dbChain = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId);

            Chain currentChain = chainRepository.FromDbChainToLibiadaChain(dbChain.id);
            String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;

            IBinaryCharacteristicCalculator calculator = BinaryCharacteristicsFactory.Create(className);
            LinkUp linkUp = (LinkUp)linkUpId;
            for (int i = 0; i < currentChain.Alphabet.Power; i++)
            {
                characteristics.Add(new List<double>());
                for (int j = 0; j < currentChain.Alphabet.Power; j++)
                {
                    long firstElementId = dbChain.alphabet.Single(a => a.number == i+1).element_id;
                    long secondElementId = dbChain.alphabet.Single(a => a.number == j+1).element_id;
                    if (
                        db.binary_characteristic.Any(b =>
                            b.chain_id == dbChain.id && b.characteristic_type_id == characteristicId &&
                            b.first_element_id == firstElementId && b.second_element_id == secondElementId &&
                            b.link_up_id == linkUpId))
                    {
                        characteristics[i].Add((double)db.binary_characteristic.Single(b =>
                            b.chain_id == dbChain.id && b.characteristic_type_id == characteristicId &&
                            b.first_element_id == firstElementId && b.second_element_id == secondElementId &&
                            b.link_up_id == linkUpId).value);
                    }
                    else
                    {
                        characteristics[i].Add(calculator.Calculate(currentChain, currentChain.Alphabet[i],
                                                                    currentChain.Alphabet[j], linkUp));
                        binary_characteristic currentCharacteristic = new binary_characteristic();
                        currentCharacteristic.chain_id = dbChain.id;
                        currentCharacteristic.characteristic_type_id = characteristicId;
                        currentCharacteristic.link_up_id = linkUpId;
                        currentCharacteristic.first_element_id = firstElementId;
                        currentCharacteristic.second_element_id = secondElementId;
                        currentCharacteristic.value = characteristics[i][j];
                        currentCharacteristic.value_string = characteristics[i][j].ToString();
                        currentCharacteristic.creation_date = DateTime.Now;
                        db.binary_characteristic.AddObject(currentCharacteristic);
                        db.SaveChanges();
                    }
                }
            }

            TempData["characteristics"] = characteristics;
            TempData["characteristicName"] = db.characteristic_type.Single(charact => charact.id == characteristicId).name;
            TempData["chainName"] = db.matter.Single(m => m.id == matterId).name;
            TempData["notationId"] = notationId;
            TempData["alphabet"] = currentChain.Alphabet;

            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            List<String> elementNames = new List<string>();

            Alphabet alpha = TempData["alphabet"] as Alphabet;
            int notationId = (int)TempData["notationId"];

            foreach (var element in alpha)
            {
                string el = element.ToString();
                elementNames.Add(db.element.Single(e => e.value == el && e.notation_id == notationId).name);
            }
            ViewBag.elementNames = elementNames;
            ViewBag.characteristics = TempData["characteristics"] as List<List<double>>;
            ViewBag.chainName = TempData["chainName"] as String;
            ViewBag.characteristicName = TempData["characteristicName"] as String;
            ViewBag.elementNames = elementNames;
            ViewBag.notationName = db.notation.Single(n => n.id == notationId).name; ;
            return View();
        }
    }
}
