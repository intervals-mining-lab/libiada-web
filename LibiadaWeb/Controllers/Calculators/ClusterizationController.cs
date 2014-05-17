namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Clusterizator.Classes;
    using Clusterizator.Classes.AlternativeClusterization;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    public class ClusterizationController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly MatterRepository matterRepository;
        private readonly ChainRepository chainRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly LinkRepository linkRepository;
        private readonly NotationRepository notationRepository;

        public ClusterizationController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            chainRepository = new ChainRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkRepository = new LinkRepository(db);
            notationRepository = new NotationRepository(db);
        }

        //
        // GET: /Clusterization/
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.full_chain_applicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetSelectListWithNature() },
                    { "characteristicTypes", characteristicTypes },
                    { "notations", notationRepository.GetSelectListWithNature() },
                    { "natures", new SelectList(db.nature, "id", "name") },
                    { "links", new SelectList(db.link, "id", "name") },
                    { "languages", new SelectList(db.language, "id", "name") },
                    { "natureLiterature", Aliases.NatureLiterature }
                };
            return View();
        }

        [HttpPost]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicIds,
            int[] linkIds,
            int[] notationIds,
            int[] languageIds,
            int clustersCount,
            double powerWeight,
            double normalizedDistanseWeight,
            double distanseWeight)
        {
            var characteristics = new List<List<double>>();
            var characteristicNames = new List<string>();
            var chainNames = new List<string>();
            foreach (var matterId in matterIds)
            {
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                characteristics.Add(new List<double>());
                for (int i = 0; i < notationIds.Length; i++)
                {
                    long chainId = db.matter.Single(m => m.id == matterId).
                        chain.Single(c => c.notation_id == notationIds[i]).id;

                    int characteristicId = characteristicIds[i];
                    int linkId = linkIds[i];
                    if (db.characteristic.Any(charact =>
                        linkId == charact.link.id &&
                        charact.chain_id == chainId &&
                        charact.characteristic_type_id == characteristicId))
                    {
                        characteristics.Last().
                        Add((double)db.characteristic.Single(charact =>
                            linkIds[i] == charact.link.id &&
                            charact.chain_id == chainId &&
                            charact.characteristic_type_id == characteristicIds[i]).value);
                    }
                    else
                    {
                        Chain tempChain = chainRepository.ToLibiadaChain(chainId);

                        string className =
                            db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                        ICalculator calculator = CalculatorsFactory.Create(className);
                        var link = (Link)db.link.Single(l => l.id == linkId).id;
                        characteristics.Last().Add(calculator.Calculate(tempChain, link));
                    }
                }
            }

            for (int k = 0; k < characteristicIds.Length; k++)
            {
                int characteristicId = characteristicIds[k];
                int linkId = linkIds[k];
                int notationId = notationIds[k];
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                    db.link.Single(l => l.id == linkId).name + " " +
                    db.notation.Single(n => n.id == notationId).name);
            }

            DataTable data = DataTableFiller.FillDataTable(matterIds.ToArray(), characteristicNames.ToArray(), characteristics);
            var clusterizator = new AlternativeKRAB(data, powerWeight, normalizedDistanseWeight, distanseWeight);
            ClusterizationResult result = clusterizator.Clusterizate(clustersCount);
            var clusters = new List<List<long>>();
            for (int i = 0; i < result.Clusters.Count; i++)
            {
                clusters.Add(new List<long>());
                foreach (var item in ((Cluster)result.Clusters[i]).Items)
                {
                    clusters.Last().Add((long)item);
                }
            }

            TempData["result"] = new Dictionary<string, object>
                                     {
                                         { "clusters", clusters },
                                         { "characteristicNames", characteristicNames },
                                         { "characteristicIds", characteristicIds },
                                         { "characteristics", characteristics },
                                         { "chainNames", chainNames },
                                         { "chainIds", new List<long>(matterIds) } 
                                     };

            return RedirectToAction("Result", "Clusterization");
        }

        public ActionResult Result()
        {
            try
            {
                var result = TempData["characteristics"] as Dictionary<string, object>;
                if (result == null)
                {
                    throw new Exception("Нет данных для отображения");
                }

                var clusters = result["clusters"] as List<List<long>>;

                var clusterNames = new List<List<string>>();
                foreach (var cluster in clusters)
                {
                    clusterNames.Add(new List<string>());
                    foreach (var matterId in cluster)
                    {
                        clusterNames.Last().Add(db.matter.Single(m => m.id == matterId).name);
                    }
                }

                var characteristicNames = result["characteristicNames"] as List<string>;

                var characteristicIds = result["characteristicIds"] as int[];
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

                ViewBag.chainNames = result["chainNames"];
                ViewBag.chainIds = result["chainIds"];
                ViewBag.characteristicNames = characteristicNames;
                ViewBag.clusters = clusters;
                ViewBag.clusterNames = clusterNames;
                ViewBag.characteristicsList = characteristicsList;
                ViewBag.characteristics = result["characteristics"];
                ViewBag.characteristicIds = new List<int>(characteristicIds);

                TempData.Keep();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);
            }
            return View();
        }
    }
}