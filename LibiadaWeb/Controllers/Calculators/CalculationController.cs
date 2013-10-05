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
    public class CalculationController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;

        public CalculationController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            List<matter> matters = db.matter.ToList();
            ViewBag.matterCheckBoxes = matterRepository.GetSelectListItems(matters, null);
            ViewBag.matters = matters;

            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => Aliases.ApplicabilityFull.Contains(c.characteristic_applicability_id));
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristicsList, null);

            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            ViewBag.languagesList = new SelectList(db.language, "id", "name");
            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] matterIds, 
            int[] characteristicIds, int[] linkUpIds, int[] notationIds, int[] languageIds)
        {
            List<List<Double>> characteristics = new List<List<Double>>();
            List<String> chainNames = new List<string>();
            List<String> characteristicNames = new List<string>();

            foreach (var matterId in matterIds)
            {
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);

                characteristics.Add(new List<Double>());
                for (int i = 0; i < notationIds.Length; i++)
                {
                    int notationId = notationIds[i];
                    int languageId = languageIds[i];
                    long chainId;
                    if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                    {
                        chainId = db.literature_chain.Single(l => l.matter_id == matterId &&
                                    l.notation_id == notationId
                                    && l.language_id == languageId).id;
                    }
                    else
                    {
                        chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                    }

                    int characteristicId = characteristicIds[i];
                    int linkUpId = linkUpIds[i];
                    if (db.characteristic.Any(c =>
                                              linkUpId == c.link_up.id && c.chain_id == chainId &&
                                              c.characteristic_type_id == characteristicId))
                    {
                        var dbCaracteristic = db.characteristic.Single(c =>
                                                                       linkUpId == c.link_up.id && c.chain_id == chainId &&
                                                                       c.characteristic_type_id == characteristicId);
                        characteristics.Last().Add(dbCaracteristic.value.Value);
                    }
                    else
                    {
                        Chain tempChain = chainRepository.ToLibiadaChain(chainId);

                        String className =
                            db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                        ICalculator calculator = CalculatorsFactory.Create(className);
                        LinkUp linkUp = (LinkUp) db.link_up.Single(l => l.id == linkUpId).id;
                        double characteristicValue = calculator.Calculate(tempChain, linkUp);
                        characteristic dbCharacteristic = new characteristic
                            {
                                chain_id = chainId,
                                characteristic_type_id = characteristicIds[i],
                                link_up_id = linkUpId,
                                value = characteristicValue,
                                value_string = characteristicValue.ToString(),
                                creation_date = DateTime.Now
                            };
                        db.characteristic.AddObject(dbCharacteristic);
                        db.SaveChanges();
                        characteristics.Last().Add(characteristicValue);
                    }
                }
            }

            for (int k = 0; k < characteristicIds.Length; k++)
            {
                int characteristicId = characteristicIds[k];
                int linkUpId = linkUpIds[k];
                int notationId = notationIds[k];
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                        db.link_up.Single(l => l.id == linkUpId).name + " " +
                                        db.notation.Single(n => n.id == notationId).name);
            }

            TempData["characteristics"] = characteristics;
            TempData["chainNames"] = chainNames;
            TempData["characteristicNames"] = characteristicNames;
            TempData["characteristicIds"] = characteristicIds;
            TempData["chainIds"] = new List<long>(matterIds);
            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            List<List<double>> characteristics = TempData["characteristics"] as List<List<double>>;
            List<String> characteristicNames = TempData["characteristicNames"] as List<String>;
            ViewBag.chainIds = TempData["chainIds"] as List<long>;
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
            ViewBag.chainNames = TempData["chainNames"] as List<String>;
            ViewBag.characteristicNames = characteristicNames;

            TempData.Keep();

            return View();
        }
    }
}
