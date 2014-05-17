namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;

    using LibiadaWeb.Models.Repositories.Catalogs;

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
        // GET: /QuickCalculation/
        public ActionResult Index()
        {
            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.full_chain_applicable);
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristicsList, null);

            ViewBag.linksList = linkRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(int[] characteristicIds, int[] linkIds, String chain)
        {
            var characteristics = new List<double>();
            var characteristicNames = new List<string>();


            for (int i = 0; i < characteristicIds.Length; i++)
            {
                var characteristicId = characteristicIds[i];
                var linkId = linkIds[i];

                var tempChain = new Chain(chain);

                characteristicNames.Add(db.characteristic_type.Single(charact => charact.id == characteristicId).name);
                var className =
                    db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                var calculator = CalculatorsFactory.Create(className);
                var link = (Link)db.link.Single(l => l.id == linkId).id;

                characteristics.Add(calculator.Calculate(tempChain, link));
            }

            TempData["characteristics"] = characteristics;
            TempData["characteristicNames"] = characteristicNames;
            TempData["characteristicIds"] = characteristicIds;
            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            var characteristics = TempData["characteristics"] as List<double>;
            var characteristicNames = TempData["characteristicNames"] as List<String>;
            var characteristicIds = TempData["characteristicIds"] as int[];
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
            ViewBag.characteristicIds = new List<int>(characteristicIds);
            ViewBag.characteristicsList = characteristicsList;
            ViewBag.characteristics = characteristics;
            ViewBag.characteristicNames = characteristicNames;

            TempData.Keep();

            return View();
        }
    }
}