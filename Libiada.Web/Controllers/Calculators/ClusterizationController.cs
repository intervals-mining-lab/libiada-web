namespace Libiada.Web.Controllers.Calculators;

using Libiada.Clusterizator;

using Libiada.Core.Music;

using Libiada.Web.Extensions;
using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Libiada.Database.Tasks;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Models.Calculators;

using Newtonsoft.Json;

using EnumExtensions = Libiada.Core.Extensions.EnumExtensions;

/// <summary>
/// The clusterization controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class ClusterizationController : AbstractResultController
{
    /// <summary>
    /// Database context factory.
    /// </summary>
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICommonSequenceRepository commonSequenceRepository;
    private readonly Cache cache;

    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClusterizationController"/> class.
    /// </summary>
    public ClusterizationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                    IViewDataHelper viewDataHelper, 
                                    ITaskManager taskManager, 
                                    IFullCharacteristicRepository characteristicTypeLinkRepository,
                                    ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator,
                                    ICommonSequenceRepository commonSequenceRepository,
                                    Cache cache) 
        : base(TaskType.Clusterization, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.commonSequenceRepository = commonSequenceRepository;
        this.cache = cache;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.sequencesCharacteristicsCalculator = sequencesCharacteristicsCalculator;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
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
        Language[] languages,
        Translator[] translators,
        PauseTreatment[] pauseTreatments,
        bool[] sequentialTransfers,
        ImageOrderExtractor[] trajectories,
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
            Dictionary<long, string> matters = cache.Matters
                                                    .Where(m => matterIds.Contains(m.Id))
                                                    .ToDictionary(m => m.Id, m => m.Name);

            long[][] sequenceIds;
            sequenceIds = commonSequenceRepository.GetSequenceIds(matterIds,
                                                                  notations,
                                                                  languages,
                                                                  translators,
                                                                  pauseTreatments,
                                                                  sequentialTransfers,
                                                                  trajectories);

            using var db = dbFactory.CreateDbContext();
            mattersNames = db.Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);

            double[][] characteristics;

            characteristics = sequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds);

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
