using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Clusterizator.Classes;
using Clusterizator.Classes.AlternativeClusterization;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Calculators
{
    public class ClusterizationController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly MatterRepository matterRepository;
        private readonly ChainRepository chainRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly NotationRepository notationRepository;

        public ClusterizationController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            chainRepository = new ChainRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            notationRepository = new NotationRepository(db);
        }

        //
        // GET: /Transformation/

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
            int[] characteristicIds, int[] linkUpIds, int[] notationIds, int[] languageIds, 
            int clustersCount, double powerWeight, double normalizedDistanseWeight, double distanseWeight)
        {
            List<List<Double>> characteristics = new List<List<Double>>();
            List<String> characteristicNames = new List<string>();
            List<String> chainNames = new List<string>();
            foreach (var matterId in matterIds)
            {
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                characteristics.Add(new List<Double>());
                for (int i = 0; i < notationIds.Length; i++)
                {
                    long chainId = db.matter.Single(m => m.id == matterId).
                        chain.Single(c => c.notation_id == notationIds[i]).id;

                    int characteristicId = characteristicIds[i];
                    int linkUpId = linkUpIds[i];
                    if (db.characteristic.Any(charact =>
                        linkUpId == charact.link_up.id &&
                        charact.chain_id == chainId &&
                        charact.characteristic_type_id == characteristicId))
                    {
                        characteristics.Last().
                        Add((double)db.characteristic.Single(charact =>
                            linkUpIds[i] == charact.link_up.id &&
                            charact.chain_id == chainId &&
                            charact.characteristic_type_id == characteristicIds[i]).value);
                    }
                    else
                    {
                        Chain tempChain = chainRepository.ToLibiadaChain(chainId);

                        String className =
                            db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                        ICalculator calculator = CalculatorsFactory.Create(className);
                        LinkUp linkUp = (LinkUp)db.link_up.Single(l => l.id == linkUpId).id;
                        characteristics.Last().Add(calculator.Calculate(tempChain, linkUp));
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

            DataTable data = DataTableFiller.FillDataTable(matterIds.ToArray(), characteristicNames.ToArray(), characteristics);
            AlternativeKRAB clusterizator = new AlternativeKRAB(data, powerWeight, normalizedDistanseWeight, distanseWeight);
            ClusterizationResult result = clusterizator.Clusterizate(clustersCount);
            List<List<long>> clusters = new List<List<long>>();
            for (int i = 0; i < result.Clusters.Count; i++)
            {
                clusters.Add(new List<long>());
                foreach (var item in ((Cluster)result.Clusters[i]).Items)
                {
                    clusters.Last().Add((long)item);
                }
            }
            TempData["clusters"] = clusters;
            TempData["characteristicNames"] = characteristicNames;
            TempData["characteristicIds"] = characteristicIds;
            TempData["characteristics"] = characteristics;
            TempData["chainIds"] = new List<long>(matterIds);
            TempData["chainNames"] = chainNames;
            return RedirectToAction("Result", "Clusterization");
        }

        public ActionResult Result()
        {
            List<List<long>> clusters = TempData["clusters"] as List<List<long>>;

            List<List<String>> clusterNames = new List<List<string>>();
            foreach (var cluster in clusters)
            {
                clusterNames.Add(new List<string>());
                foreach (var matterId in cluster)
                {
                    clusterNames.Last().Add(db.matter.Single(m => m.id == matterId).name);
                }
            }
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
            ViewBag.chainNames = TempData["chainNames"] as List<String>;
            ViewBag.chainIds = TempData["chainIds"] as List<long>;
            ViewBag.characteristicNames = characteristicNames;
            ViewBag.clusters = clusters;
            ViewBag.clusterNames = clusterNames;
            ViewBag.characteristicsList = characteristicsList;
            ViewBag.characteristics = TempData["characteristics"] as List<List<Double>>;
            ViewBag.characteristicIds = new List<int>(characteristicIds);

            TempData.Keep();

            return View();
        }
    }
}
