namespace Libiada.Web.Controllers.Calculators;

using Newtonsoft.Json;

using Libiada.Core.TimeSeries.Aggregators;
using Libiada.Core.TimeSeries.Aligners;
using Libiada.Core.TimeSeries.OneDimensional.Comparers;
using Libiada.Core.TimeSeries.OneDimensional.DistanceCalculators;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Calculators;

using Libiada.Web.Tasks;

/// <summary>
/// The local calculation web api controller.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class LocalCalculationWebApiController : ControllerBase
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;
    private readonly IFullCharacteristicRepository fullCharacteristicRepository;
    private readonly ITaskManager taskManager;

    public LocalCalculationWebApiController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                            ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory,
                                            IFullCharacteristicRepository fullCharacteristicRepository,
                                            ITaskManager taskManager)
    {
        this.dbFactory = dbFactory;
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
        this.fullCharacteristicRepository = fullCharacteristicRepository;
        this.taskManager = taskManager;
    }

    /// <summary>
    /// The get subsequence characteristic.
    /// </summary>
    /// <param name="subsequenceId">
    /// The subsequence id.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
    /// </param>
    /// <param name="windowSize">
    /// The window size.
    /// </param>
    /// <param name="step">
    /// The step.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    [HttpGet]
    public string GetSubsequenceCharacteristic(
        long subsequenceId,
        short characteristicLinkId,
        int windowSize,
        int step)
    {
        using var db = dbFactory.CreateDbContext();
        using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
        var calculator = new LocalCharacteristicsCalculator(db, fullCharacteristicRepository, commonSequenceRepository);
        var characteristics = calculator.GetSubsequenceCharacteristic(subsequenceId, characteristicLinkId, windowSize, step);

        return JsonConvert.SerializeObject(characteristics);
    }

    [HttpGet]
    public string CalculateLocalCharacteristicsSimilarityMatrix(
        int taskId,
        Aligner aligner,
        DistanceCalculator distanceCalculator,
        Aggregator aggregator)
    {
        var data = taskManager.GetTaskData(taskId);

        var characteristicsObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);
        var characteristics = characteristicsObject["characteristics"];
        LocalCharacteristicsData[] chars = characteristics.ToObject<LocalCharacteristicsData[]>();

        var series = new double[chars.Length][];

        for (int i = 0; i < chars.Length; i++)
        {
            series[i] = chars[i].FragmentsData.Select(fd => fd.Characteristics[0]).ToArray();
        }

        var alignersFactory = new AlignersFactory();
        var calculatorsFactory = new DistanceCalculatorsFactory();
        var aggregatorsFactory = new AggregatorsFactory();

        var comparer = new OneDimensionalTimeSeriesComparer(
            alignersFactory.GetAligner(aligner),
            calculatorsFactory.GetDistanceCalculator(distanceCalculator),
            aggregatorsFactory.GetAggregator(aggregator));

        var result = new double[series.Length, series.Length];

        for (int i = 0; i < series.Length - 1; i++)
        {
            var firstSeries = series[i];
            for (int j = i + 1; j < series.Length; j++)
            {
                var secondSeries = series[j];
                result[i,j] = comparer.GetDistance(firstSeries, secondSeries);
                result[j,i] = result[i,j];
            }
        }

        var response = new Dictionary<string, object>
        {
            { "aligner", aligner },
            { "distanceCalculator", distanceCalculator },
            { "aggregator", aggregator },
            { "result", result }
        };

        return JsonConvert.SerializeObject(response);
    }
}
