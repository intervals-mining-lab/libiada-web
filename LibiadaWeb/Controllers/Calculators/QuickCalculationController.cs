using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories.Catalogs;

namespace LibiadaWeb.Controllers.Calculators
{
    public class QuickCalculationController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly LinkRepository linkRepository;

        public QuickCalculationController()
        {
            db = new LibiadaWebEntities();
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkRepository = new LinkRepository(db);
        }

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => Aliases.ApplicabilityFull.Contains(c.characteristic_applicability_id));
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristicsList, null);

            ViewBag.linksList = linkRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(int[] characteristicIds, int[] linkIds, String chain)
        {
            List<Double> characteristics = new List<Double>();
            List<String> characteristicNames = new List<string>();


            for (int i = 0; i < characteristicIds.Length; i++)
            {
                int characteristicId = characteristicIds[i];
                int linkId = linkIds[i];

                Chain tempChain = new Chain(chain);

                characteristicNames.Add( db.characteristic_type.Single(charact => charact.id == characteristicId).name);
                String className =
                    db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                ICalculator calculator = CalculatorsFactory.Create(className);
                Link link = (Link) db.link.Single(l => l.id == linkId).id;

                characteristics.Add(calculator.Calculate(tempChain, link));
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

            TempData.Keep();

            return View();
        }

    }
}
