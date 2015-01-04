namespace LibiadaWeb.Controllers.Calculators
{
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
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The clusterization controller.
    /// </summary>
    public class ClusterizationController : AbstractResultController
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

            ControllerName = "Clusterization";
            DisplayName = "Clusterization";
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

            var links = new SelectList(db.link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Нет" });

            var translators = new SelectList(db.translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", this.matterRepository.GetSelectListWithNature() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", this.notationRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(db.nature, "id", "name") }, 
                    { "links", links }, 
                    { "languages", new SelectList(db.language, "id", "name") }, 
                    { "translators", translators }
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
            int?[] linkIds,
            int[] notationIds,
            int[] languageIds,
            int clustersCount,
            double powerWeight,
            double normalizedDistanseWeight,
            double distanseWeight)
        {
            return Action(() =>
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
                        int? linkId = linkIds[i];
                        if (db.characteristic.Any(c =>
                            ((linkId == null && c.link_id == null) || (linkId == c.link_id)) &&
                            c.chain_id == chainId &&
                            c.characteristic_type_id == characteristicId))
                        {
                            characteristics.Last().
                                Add((double)db.characteristic.Single(c =>
                                    ((linkId == null && c.link_id == null) || (linkId == c.link_id)) &&
                                    c.chain_id == chainId &&
                                    c.characteristic_type_id == characteristicIds[i]).value);
                        }
                        else
                        {
                            Chain tempChain = this.chainRepository.ToLibiadaChain(chainId);

                            string className =
                                db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                            var link = linkId != null ? (Link)db.link.Single(l => l.id == linkId).id : Link.None;
                            characteristics.Last().Add(calculator.Calculate(tempChain, link));
                        }
                    }
                }

                for (int k = 0; k < characteristicIds.Length; k++)
                {
                    int characteristicId = characteristicIds[k];
                    int? linkId = linkIds[k];
                    int notationId = notationIds[k];

                    string linkName = linkId != null ? db.link.Single(l => l.id == linkId).name : string.Empty;

                    characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                            linkName + " " +
                                            db.notation.Single(n => n.id == notationId).name);
                }

                DataTable data = DataTableFiller.FillDataTable(matterIds.ToArray(), characteristicNames.ToArray(),
                    characteristics);
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

                var clusterNames = new List<List<string>>();
                foreach (var cluster in clusters)
                {
                    clusterNames.Add(new List<string>());
                    foreach (var matterId in cluster)
                    {
                        clusterNames.Last().Add(db.matter.Single(m => m.id == matterId).name);
                    }
                }

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

                return new Dictionary<string, object>
                {
                    {"clusters", clusters},
                    {"characteristicNames", characteristicNames},
                    {"characteristicIds", new List<int>(characteristicIds)},
                    {"characteristics", characteristics},
                    {"chainNames", chainNames},
                    {"chainIds", new List<long>(matterIds)},
                    {"clusterNames", clusterNames},
                    {"characteristicsList", characteristicsList}
                };
            });
        }
    }
}