using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers
{
    public class QuickCalculationController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly LinkUpRepository linkUpRepository;

        public QuickCalculationController()
        {
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkUpRepository = new LinkUpRepository(db);
        }

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            ViewBag.characteristics = db.characteristic_type.ToList();

            ViewBag.linkUps = db.link_up.ToList();
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(int[] characteristicIds, int[] linkUpIds, String chain)
        {
            List<Double> characteristics = new List<Double>();
            List<String> characteristicNames = new List<string>();


            for (int i = 0; i < characteristicIds.Length; i++)
            {
                int characteristicId = characteristicIds[i];
                int linkUpId = linkUpIds[i];

                Chain tempChain = new Chain(chain);

                characteristicNames.Add( db.characteristic_type.Single(charact => charact.id == characteristicId).name);
                String className =
                    db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                ICharacteristicCalculator calculator = CharacteristicsFactory.Create(className);
                LinkUp linkUp = (LinkUp) db.link_up.Single(l => l.id == linkUpId).id;

                characteristics.Add(calculator.Calculate(tempChain, linkUp));
            }

            TempData["characteristics"] = characteristics;
            TempData["characteristicNames"] = characteristicNames;
            TempData["characteristicIds"] = characteristicIds;
            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            List<double> characteristics = TempData["characteristics"] as List<double>;
            List<String> characteristicNames = TempData["characteristicNames"] as List<String>;
            int[] characteristicIds = TempData["characteristicIds"] as int[];
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
            ViewBag.characteristicIds = new List<int>(characteristicIds);
            ViewBag.characteristicsList = characteristicsList;
            ViewBag.characteristics = characteristics;
            ViewBag.characteristicNames = characteristicNames;
            return View();
        }

    }
}
