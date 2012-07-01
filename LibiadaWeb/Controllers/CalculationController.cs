using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;
using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers
{
    public class CalculationController : Controller
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
        public ActionResult Index(long[] chainIds, int[] characteristicIds, int linkUp)
        {
            List<List<Double>> characteristics = new List<List<Double>>();
            List<String> chainNames = new List<string>();
            List<String> characteristicNames = new List<string>();

            foreach (var chainId in chainIds)
            {
                chainNames.Add(db.chain.Include("matter").Single(c => c.id == chainId).matter.name);

                characteristics.Add(new List<Double>());
                foreach (var characteristicId in characteristicIds)
                {

                    if (db.characteristic.Any(charact => 
                        linkUp == charact.link_up.id && 
                        charact.chain_id == chainId && 
                        charact.characteristic_type_id == characteristicId))
                    {
                        characteristics.Last().
                        Add((double)db.characteristic.Single(charact =>
                            linkUp == charact.link_up.id &&
                            charact.chain_id == chainId &&
                            charact.characteristic_type_id == characteristicId).value);
                    }
                    else
                    {
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
                        ICharacteristicCalculator calculator = CharacteristicsFactory.Create(className);
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
                        characteristics.Last().Add(calculator.Calculate(currentChain, link));
                    }
                }
            }

            foreach (var characteristicId in characteristicIds)
            {
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name);
            }

            TempData["characteristics"] = characteristics;
            TempData["chainNames"] = chainNames;
            TempData["characteristicNames"] = characteristicNames;
            
            return  RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            List<List<double>> characteristics = TempData["characteristics"] as List<List<double>>;
            List<String> chainNames = TempData["chainNames"] as List<String>;
            List<String> characteristicNames = TempData["characteristicNames"] as List<String>;
            ViewBag.characteristics = characteristics;
            ViewBag.chainNames = chainNames;
            ViewBag.characteristicNames = characteristicNames;
            return View();
        }
    }
}
