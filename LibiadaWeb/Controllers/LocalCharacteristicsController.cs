using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using LibiadaCore.Classes.Misc.Iterators;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers
{
    public class LocalCharacteristicsController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();
        private MatterRepository matterRepository = new MatterRepository();
        private CharacteristicTypeRepository characteristicRepository = new CharacteristicTypeRepository();
        private NotationRepository notationRepository = new NotationRepository();
        private LinkUpRepository linkUpRepository = new LinkUpRepository();
        private ChainRepository chainRepository = new ChainRepository();


        //
        // GET: /Transformation/

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
        public ActionResult Index(long matterId, int[] characteristicIds, int[] linkUpIds, int[] notationIds, int length, int step)
        {
            List<List<Double>> characteristicsTemp = new List<List<Double>>();
            String chainName = db.matter.Single(m => m.id == matterId).name;
            List<String> partNames = new List<string>();
            List<String> characteristicNames = new List<string>();
            characteristicsTemp.Add(new List<Double>());
            for (int i = 0; i < notationIds.Length; i++)
            {
                matter matter = db.matter.Single(m => m.id == matterId);
                chain chain = matter.chain.Single(c => c.building_type_id == 1 && c.notation_id == notationIds[i]);
                Chain libiadaChain = chainRepository.FromDbChainToLibiadaChain(chain);
                int characteristicId = characteristicIds[i];
                int linkUpId = linkUpIds[i];
                int notationId = notationIds[i];
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                        db.link_up.Single(l => l.id == linkUpId).name + " " +
                                        db.notation.Single(n => n.id == notationId).name);

                String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                ICharacteristicCalculator calculator = CharacteristicsFactory.Create(className);
                LinkUp linkUp = (LinkUp)db.link_up.Single(l => l.id == linkUpId).id;

                IteratorStart<Chain, Chain> iter = new IteratorStart<Chain, Chain>(libiadaChain, length, step);
                while (iter.Next())
                {
                    Chain tempChain = iter.Current();
                    partNames.Add(tempChain.ToString());
                    
                    double characteristicValue = calculator.Calculate(tempChain, linkUp);
                    characteristicsTemp.Last().Add(calculator.Calculate(tempChain, linkUp));
                }
            }

            List<List<Double>> characteristics = new List<List<Double>>();

            for (int t = 0; t < characteristicsTemp[0].Count; t++)
            {
                characteristics.Add(new List<double>());
                for (int w = 0; w < characteristicsTemp.Count; w++)
                {
                    characteristics[t].Add(characteristicsTemp[w][t]);
                }
            }

            TempData["characteristics"] = characteristics;
            TempData["chainName"] = chainName;
            TempData["partNames"] = partNames;
            TempData["characteristicNames"] = characteristicNames;
            TempData["characteristicIds"] = characteristicIds;
            TempData["chainIds"] = matterId;
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
            ViewBag.chainName = TempData["chainName"] as String;
            ViewBag.partNames = TempData["partNames"] as List<String>;
            ViewBag.characteristicNames = characteristicNames;
            return View();
        }
    }
}
