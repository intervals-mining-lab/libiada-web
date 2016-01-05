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
    using LibiadaWeb.Models.Repositories.Sequences;
    using Math;

    using Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    /// <summary>
    /// The clusterization controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ClusterizationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterizationController"/> class.
        /// </summary>
        public ClusterizationController() : base("Clusterization")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(c => c.FullSequenceApplicable, 3, int.MaxValue, true, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type and link ids.
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
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicTypeLinkIds,
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

                        int characteristicTypeLinkId = characteristicTypeLinkIds[i];
                        if (db.Characteristic.Any(c => c.SequenceId == sequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId))
                        {
                            characteristics.Last()
                                .Add(db.Characteristic.Single(c => c.SequenceId == sequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId).Value);
                        }
                        else
                        {
                            Chain tempChain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                            characteristics.Last().Add(calculator.Calculate(tempChain, link));
                        }
                    }
                }

                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k], notationIds[k]));
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
                    { "characteristicIds", new List<int>(characteristicTypeLinkIds) },
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
