using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;
using LibiadaWeb.Models;
using NewClusterization.Classes.DataMining.Clusterization;
using NewClusterization.Classes.DataMining.Clusterization.AlternativeClusterization;

namespace LibiadaWeb.Controllers
{
    public class ClusterizationController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();
        private MatterRepository matterRepository = new MatterRepository();
        private ChainRepository chainRepository = new ChainRepository();
        private CharacteristicTypeRepository characteristicRepository = new CharacteristicTypeRepository();
        private LinkUpRepository linkUpRepository = new LinkUpRepository();
        private NotationRepository notationRepository = new NotationRepository();

        //
        // GET: /Transformation/

        public ActionResult Index()
        {
            ViewBag.characteristics = db.characteristic_type.ToList();
            ViewBag.linkUps = db.link_up.ToList();
            ViewBag.notations = db.notation.ToList();
            ViewBag.objects = db.matter.Include("chain").ToList();
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] matterIds, int[] characteristicIds, int[] linkUpIds, int[] notationIds, int clustersCount, double powerWeight, double normalizedDistanseWeight, double distanseWeight)
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
                        chain.Single(c => c.building_type_id == 1 && c.notation_id == notationIds[i]).id;

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

            DataTable data = DataTableFiller.FillDataTable(matterIds.ToArray(), characteristicNames.ToArray(), characteristics);
            AlternativeKRAB clusterizator = new AlternativeKRAB(data, powerWeight, normalizedDistanseWeight, distanseWeight);
            ClusterizationResult result = clusterizator.Clusterizate(clustersCount);
            List<List<long>> clusters = new List<List<long>>();
            for (int i = 0; i < result.Clusters.Count;i++)
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
            return View();
        }
    }
}
