namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Clusterizator;
    using Clusterizator.Krab;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaWeb.Models.Repositories.Sequences;
    using Math;

    using Models.Repositories.Catalogs;

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
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

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
        public ClusterizationController() : base("Clusterization", "Clusterization")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkRepository = new LinkRepository(db);
            notationRepository = new NotationRepository(db);
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
            var characteristicsList = db.CharacteristicType.Where(c => c.FullSequenceApplicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var links = new SelectList(db.Link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(db.Nature, "id", "name") }, 
                    { "links", links }, 
                    { "languages", new SelectList(db.Language, "id", "name") }, 
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
        /// <param name="equipotencyWeight">
        /// The power weight.
        /// </param>
        /// <param name="normalizedDistanceWeight">
        /// The normalized distance weight.
        /// </param>
        /// <param name="distanceWeight">
        /// The distance weight.
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
            double equipotencyWeight,
            double normalizedDistanceWeight,
            double distanceWeight)
        {
            return Action(() =>
            {
                var characteristics = new List<List<double>>();
                var characteristicNames = new List<string>();
                var sequenceNames = new List<string>();
                foreach (var matterId in matterIds)
                {
                    sequenceNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    characteristics.Add(new List<double>());
                    for (int i = 0; i < notationIds.Length; i++)
                    {
                        long sequenceId = db.Matter.Single(m => m.Id == matterId).Sequence.Single(c => c.NotationId == notationIds[i]).Id;

                        int characteristicId = characteristicIds[i];
                        int? linkId = linkIds[i];
                        if (db.Characteristic.Any(c =>
                            ((linkId == null && c.LinkId == null) || (linkId == c.LinkId)) &&
                            c.SequenceId == sequenceId &&
                            c.CharacteristicTypeId == characteristicId))
                        {
                            characteristics.Last()
                                .Add((double)db.Characteristic.Single(c =>
                                    ((linkId == null && c.LinkId == null) || (linkId == c.LinkId)) &&
                                    c.SequenceId == sequenceId &&
                                    c.CharacteristicTypeId == characteristicIds[i]).Value);
                        }
                        else
                        {
                            Chain tempChain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                            string className =
                                db.CharacteristicType.Single(c => c.Id == characteristicId).ClassName;
                            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                            var link = (Link)(linkId ?? 0);
                            characteristics.Last().Add(calculator.Calculate(tempChain, link));
                        }
                    }
                }

                for (int k = 0; k < characteristicIds.Length; k++)
                {
                    int characteristicId = characteristicIds[k];
                    int? linkId = linkIds[k];
                    int notationId = notationIds[k];

                    string linkName = linkId != null ? db.Link.Single(l => l.Id == linkId).Name : string.Empty;

                    characteristicNames.Add(db.CharacteristicType.Single(c => c.Id == characteristicId).Name + " " +
                                            linkName + " " +
                                            db.Notation.Single(n => n.Id == notationId).Name);
                }

                DataTable data = DataTableFiller.FillDataTable(matterIds.ToArray(), characteristicNames.ToArray(), characteristics);
                var clusterizator = new KrabClusterization(data, equipotencyWeight, normalizedDistanceWeight, distanceWeight);
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
                        clusterNames.Last().Add(db.Matter.Single(m => m.Id == matterId).Name);
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
                    { "clusters", clusters },
                    { "characteristicNames", characteristicNames },
                    { "characteristicIds", new List<int>(characteristicIds) },
                    { "characteristics", characteristics },
                    { "sequenceNames", sequenceNames },
                    { "matterIds", new List<long>(matterIds) },
                    { "clusterNames", clusterNames },
                    { "characteristicsList", characteristicsList }
                };
            });
        }
    }
}
