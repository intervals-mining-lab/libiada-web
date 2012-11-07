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
    public class HomogeneousCalculationController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;


        public HomogeneousCalculationController()
        {
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            chainRepository = new ChainRepository(db);
        }

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
        public ActionResult Index(long matterId, int[] characteristicIds, int[] linkUpIds, int notationId)
        {
            List<List<Double>> characteristicsTemp = new List<List<Double>>();
            String chainName = db.matter.Single(m => m.id == matterId).name;
            List<String> partNames = new List<string>();
            List<String> characteristicNames = new List<string>();

            for (int i = 0; i < characteristicIds.Length; i++)
            {
                characteristicsTemp.Add(new List<Double>());
                matter matter = db.matter.Single(m => m.id == matterId);
                chain chain = matter.chain.Single(c => c.building_type_id == 1 && c.notation_id == notationId);
                Chain libiadaChain = chainRepository.FromDbChainToLibiadaChain(chain);
                int characteristicId = characteristicIds[i];
                int linkUpId = linkUpIds[i];
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                        db.link_up.Single(l => l.id == linkUpId).name + " " +
                                        db.notation.Single(n => n.id == notationId).name);

                String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                ICharacteristicCalculator calculator = CharacteristicsFactory.Create(className);
                LinkUp linkUp = (LinkUp)db.link_up.Single(l => l.id == linkUpId).id;


                for (int j = 0; j < libiadaChain.Alphabet.Power; j++)
                {
                    UniformChain tempChain = libiadaChain.GetUniformChain(j);
                    partNames.Add(tempChain.ToString());
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
