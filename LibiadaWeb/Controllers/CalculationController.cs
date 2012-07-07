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
        private MatterRepository matterRepository = new MatterRepository();
        private CharacteristicTypeRepository characteristicRepository = new CharacteristicTypeRepository();
        NotationRepository notationRepository = new NotationRepository();
        private LinkUpRepository linkUpRepository = new LinkUpRepository();

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            ViewBag.characteristics = db.characteristic_type.ToList();

            ViewBag.linkUps = db.link_up.ToList();
            ViewBag.notations = db.notation.ToList();
            ViewBag.objects = db.matter.Include("chain").ToList();
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(null);
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] matterIds, int[] characteristicIds, int[] linkUpIds, int[] notationIds)
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
                    long chainId = db.matter.Single(m => m.id == matterId).
                        chain.Single(c => c.building_type_id == 1 && c.notation_id == notationIds[i]).id;

                    int characteristicId = characteristicIds[i];
                    int linkUpId = linkUpIds[i];
                    if (db.characteristic.Any(charact =>
                                              linkUpId == charact.link_up.id &&
                                              charact.chain_id == chainId &&
                                              charact.characteristic_type_id == characteristicId))
                    {
                        characteristics.Last().
                            Add((double) db.characteristic.Single(charact =>
                                                                  linkUpIds[i] == charact.link_up.id &&
                                                                  charact.chain_id == chainId &&
                                                                  charact.characteristic_type_id == characteristicIds[i])
                                             .value);
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

                        Chain tempChain = new Chain(db.chain.Single(c => c.id == chainId).building, alpha);

                        String className =
                            db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                        ICharacteristicCalculator calculator = CharacteristicsFactory.Create(className);
                        LinkUp link = LinkUp.End;
                        switch (db.link_up.Single(l => l.id == linkUpId).id)
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
                        characteristics.Last().Add(calculator.Calculate(tempChain, link));
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
            return  RedirectToAction("Result");
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
            return View();
        }
    }
}
