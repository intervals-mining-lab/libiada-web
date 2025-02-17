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
using Libiada.Database.Models.CalculatorsData;

using Newtonsoft.Json;

using EnumExtensions = Core.Extensions.EnumExtensions;


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
    private readonly IViewDataBuilder viewDataBuilder;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClusterizationController"/> class.
    /// </summary>
    public ClusterizationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                    IViewDataBuilder viewDataBuilder,
                                    ITaskManager taskManager,
                                    IFullCharacteristicRepository characteristicTypeLinkRepository,
                                    ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator,
                                    ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                    IResearchObjectsCache cache)
        : base(TaskType.Clusterization, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataBuilder = viewDataBuilder;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
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
        var viewData = viewDataBuilder.AddMinMaxResearchObjects(3, int.MaxValue)
                                      .AddSequenceGroups()
                                      .AddNatures()
                                      .AddNotations()
                                      .AddLanguages()
                                      .AddTranslators()
                                      .AddPauseTreatments()
                                      .AddTrajectories()
                                      .AddSequenceTypes()
                                      .AddGroups()
                                      .AddCharacteristicsData(CharacteristicCategory.Full)
                                      .Build();
        viewData.Add("ClusterizatorsTypes", EnumExtensions.ToArray<ClusterizationType>().ToSelectList());
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectIds">
    /// The research objects ids.
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
    public ActionResult Index(
        long[] researchObjectIds,
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
            Dictionary<long, string> researchObjectsNames = cache.ResearchObjects
                                                    .Where(m => researchObjectIds.Contains(m.Id))
                                                    .ToDictionary(m => m.Id, m => m.Name);

            using var sequenceRepository = sequenceRepositoryFactory.Create();
            long[][] sequenceIds;
            sequenceIds = sequenceRepository.GetSequenceIds(researchObjectIds,
                                                            notations,
                                                            languages,
                                                            translators,
                                                            pauseTreatments,
                                                            sequentialTransfers,
                                                            trajectories);

            using var db = dbFactory.CreateDbContext();

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
            var researchObjectsCharacteristics = new SequenceCharacteristics[researchObjectIds.Length];
            for (int i = 0; i < clusterizationResult.Length; i++)
            {
                researchObjectsCharacteristics[i] = new SequenceCharacteristics
                {
                    ResearchObjectName = researchObjectsNames[researchObjectIds[i]],
                    SequenceGroupId = clusterizationResult[i] + 1,
                    Characteristics = characteristics[i]
                };
            }

            string[] characteristicNames = new string[characteristicLinkIds.Length];
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

            var actualClustersCount = clusterizationResult.Distinct().Count();

            IEnumerable<SelectListItem> sequenceGroupsSelectlist = Enumerable.Range(0, actualClustersCount)
            .Select(i => new SelectListItem
            {
                Text = $"Cluster {i + 1}",
                Value = (i + 1).ToString(),
            });

            var result = new Dictionary<string, object>
            {
                { "characteristicNames", characteristicNames },
                { "characteristics", researchObjectsCharacteristics },
                { "characteristicsList", characteristicsList },
                { "sequenceGroups", sequenceGroupsSelectlist }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
