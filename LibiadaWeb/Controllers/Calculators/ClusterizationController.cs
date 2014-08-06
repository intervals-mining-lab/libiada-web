namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Clusterizator;
    using Clusterizator.Krab;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Math;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The clusterization controller.
    /// </summary>
    public class ClusterizationController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The link repository.
        /// </summary>
        private readonly LinkRepository linkRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterizationController"/> class.
        /// </summary>
        public ClusterizationController()
        {
            db = new LibiadaWebEntities();
            this.matterRepository = new MatterRepository(db);
            this.chainRepository = new ChainRepository(db);
            this.characteristicRepository = new CharacteristicTypeRepository(db);
            this.linkRepository = new LinkRepository(db);
            this.notationRepository = new NotationRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.full_chain_applicable);

            var characteristicTypes = this.characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", this.matterRepository.GetSelectListWithNature() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", this.notationRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(db.nature, "id", "name") }, 
                    { "links", new SelectList(db.link, "id", "name") }, 
                    { "languages", new SelectList(db.language, "id", "name") }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="languageIds">
        /// The language ids.
        /// </param>
        /// <param name="clustersCount">
        /// The clusters count.
        /// </param>
        /// <param name="powerWeight">
        /// The power weight.
        /// </param>
        /// <param name="normalizedDistanseWeight">
        /// The normalized distanse weight.
        /// </param>
        /// <param name="distanseWeight">
        /// The distanse weight.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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
                        Chain tempChain = this.chainRepository.ToLibiadaChain(chainId);

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
            var clusterizator = new KrabClusterization(data, powerWeight, normalizedDistanseWeight, distanseWeight);
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

            this.TempData["result"] = new Dictionary<string, object>
                                     {
                                         { "clusters", clusters }, 
                                         { "characteristicNames", characteristicNames }, 
                                         { "characteristicIds", characteristicIds }, 
                                         { "characteristics", characteristics }, 
                                         { "chainNames", chainNames }, 
                                         { "chainIds", new List<long>(matterIds) } 
                                     };

            return this.RedirectToAction("Result", "Clusterization");
        }

        /// <summary>
        /// The result.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public ActionResult Result()
        {
            try
            {
                var result = this.TempData["characteristics"] as Dictionary<string, object>;
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

                this.TempData.Keep();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("Error", e.Message);
            }

            return View();
        }
    }
}