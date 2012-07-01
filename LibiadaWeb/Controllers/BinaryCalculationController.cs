using System;
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
        private ChainRepository chainRepository = new ChainRepository();
        private CharacteristicTypeRepository characteristicsRepository = new CharacteristicTypeRepository();
        private LinkUpRepository linkUpRepository = new LinkUpRepository();

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            ViewBag.chains = db.chain.Include("building_type").Include("matter").Include("notation").ToList();

            ViewBag.chainsList = chainRepository.GetSelectListItems(null);
            ViewBag.characteristicsList = characteristicsRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long chainId, int characteristicId, int linkUp)
        {
            List<List<Double>> characteristics = new List<List<Double>>();
            
            List<String> elementNames = new List<string>();

            String chainName = db.chain.Include("matter").Single(c => c.id == chainId).matter.name;
            int chainNotationId = db.chain.Single(c => c.id == chainId).notation_id;

            characteristics.Add(new List<Double>());
            Alphabet alpha = new Alphabet();
            IEnumerable<element> elements =
                db.alphabet.Where(a => a.chain_id == chainId).Select(a => a.element);
            foreach (var element in elements)
            {
                alpha.Add(new ValueString(element.value));
            }

            Chain currentChain = new Chain(db.chain.Single(c => c.id == chainId).building, alpha);

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

            foreach (var element in currentChain.Alphabet)
            {
                string el = element.ToString();
                elementNames.Add(db.element.Single(e => e.value == el && e.notation_id == chainNotationId).name);
            }

            TempData["characteristics"] = characteristics;
            TempData["chainName"] = chainName;
            TempData["elementNames"] = elementNames;

            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            List<List<double>> characteristics = TempData["characteristics"] as List<List<double>>;
            String chainName = TempData["chainName"] as String;
            List<String> elementNames = TempData["elementNames"] as List<String>;
            ViewBag.characteristics = characteristics;
            ViewBag.chainName = chainName;
            ViewBag.elementNames = elementNames;
            return View();
        }
    }
}
