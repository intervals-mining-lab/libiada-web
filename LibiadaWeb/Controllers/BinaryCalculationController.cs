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
        public ActionResult Index(long matterId, int characteristicId, int linkUp, int notationId)
        {
            List<List<Double>> characteristics = new List<List<Double>>();
            
            List<String> elementNames = new List<String>();

            Alphabet alpha = new Alphabet();
            alpha.Add(NullValue.Instance());
            long chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
            IEnumerable<element> elements =
                db.alphabet.Where(a => a.chain_id == chainId).Select(a => a.element);
            foreach (var element in elements)
            {
                alpha.Add(new ValueString(element.value));
            }

            Chain currentChain = new Chain(db.chain.Single(c => c.id == chainId).building.OrderBy(b => b.index).Select(b => b.number).ToArray(), alpha);

            String className =
                db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                
            IBinaryCharacteristicCalculator calculator = BinaryCharacteristicsFactory.Create(className);
            LinkUp link = LinkUp.End;
            switch (db.link_up.Single(l => l.id == linkUp).id)
            {
                case 1:
                    link = LinkUp.Start;
                    break;
                case 2:
                    link = LinkUp.End;
                    break;
                case 3:
                    link = LinkUp.Both;
                    break;
            }
            characteristics = calculator.Calculate(currentChain, link);



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
            ViewBag.notationName = db.notation.Single(n => n.id == notationId).name;;
            return View();
        }
    }
}
