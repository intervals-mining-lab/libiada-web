namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Clusterizator;

    using LibiadaCore.Music;
    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

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
        private readonly FullCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterizationController"/> class.
        /// </summary>
        public ClusterizationController() : base(TaskType.Clusterization)
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
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
            Dictionary<string, object> viewData = viewDataHelper.FillViewData(CharacteristicCategory.Full, 3, int.MaxValue, "Calculate");
            viewData.Add("ClusterizatorsTypes", EnumExtensions.ToArray<ClusterizationType>().ToSelectList());
            ViewBag.data = JsonConvert.SerializeObject(viewData);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="notations">
        /// The notation ids.
        /// </param>
        /// <param name="languages">
        /// The language ids.
        /// </param>
        /// <param name="translators">
        /// The translators ids.
        /// </param>
        /// <param name="pauseTreatments">
        /// Pause treatment parameters of music sequences.
        /// </param>
        /// <param name="sequentialTransfers">
        /// Sequential transfer flag used in music sequences.
        /// </param>
        /// <param name="trajectories">
        /// Reading trajectories for images.
        /// </param>
        /// <param name="clustersCount">
        /// The clusters count.
        /// Minimum clusters count for methods
        /// that use range of clusters.
        /// </param>
        /// <param name="clusterizationType">
        /// Clusterization method.
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
        /// <param name="bandwidth">
        /// The bandwidth.
        /// </param>
        /// <param name="maximumClusters">
        /// The maximum clusters count
        /// for methods that use range of possible custers.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            short[] characteristicLinkIds,
            Notation[] notations,
            Language?[] languages,
            Translator?[] translators,
            PauseTreatment?[] pauseTreatments,
            bool?[] sequentialTransfers,
            ImageOrderExtractor?[] trajectories,
            int clustersCount,
            ClusterizationType clusterizationType,
            double equipotencyWeight = 1,
            double normalizedDistanceWeight = 1,
            double distanceWeight = 1,
            double bandwidth = 0,
            int maximumClusters = 2)
        {
            return CreateTask(() =>
            {
                Dictionary<long, string> mattersNames;
                Dictionary<long, string> matters = Cache.GetInstance()
                                                        .Matters
                                                        .Where(m => matterIds.Contains(m.Id))
                                                        .ToDictionary(m => m.Id, m => m.Name);

                long[][] sequenceIds;
                using (var db = new LibiadaWebEntities())
                {
                    var commonSequenceRepository = new CommonSequenceRepository(db);
                    sequenceIds = commonSequenceRepository.GetSequenceIds(matterIds,
                                                                          notations,
                                                                          languages,
                                                                          translators,
                                                                          pauseTreatments,
                                                                          sequentialTransfers,
                                                                          trajectories);
                    mattersNames = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
                }

                double[][] characteristics;

                characteristics = SequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds);

                var clusterizationParams = new Dictionary<string, double>
                {
                    { "clustersCount", clustersCount },
                    { "equipotencyWeight", equipotencyWeight },
                    { "normalizedDistanceWeight", normalizedDistanceWeight },
                    { "distanceWeight", distanceWeight },
                    { "bandwidth", bandwidth },
                    { "maximumClusters", maximumClusters }
                };

                IClusterizator clusterizator = ClusterizatorsFactory.CreateClusterizator(clusterizationType, clusterizationParams);
                int[] clusterizationResult = clusterizator.Cluster(clustersCount, characteristics);
                var mattersCharacteristics = new object[matterIds.Length];
                for (int i = 0; i < clusterizationResult.Length; i++)
                {
                    mattersCharacteristics[i] = new
                    {
                        MatterName = mattersNames[matterIds[i]],
                        cluster = clusterizationResult[i] + 1,
                        Characteristics = characteristics[i]
                    };
                }

                var characteristicNames = new string[characteristicLinkIds.Length];
                var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
                    characteristicsList[k] = new SelectListItem
                    {
                        Value = k.ToString(),
                        Text = characteristicNames[k],
                        Selected = false
                    };
                }

                var result = new Dictionary<string, object>
                {
                    { "characteristicNames", characteristicNames },
                    { "characteristics", mattersCharacteristics },
                    { "characteristicsList", characteristicsList },
                    { "clustersCount", clusterizationResult.Distinct().Count() }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}
