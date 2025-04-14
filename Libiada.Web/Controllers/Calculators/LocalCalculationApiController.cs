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
public class LocalCalculationApiController : ControllerBase
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly IFullCharacteristicRepository fullCharacteristicRepository;
    private readonly ITaskManager taskManager;

    public LocalCalculationApiController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                            ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                            IFullCharacteristicRepository fullCharacteristicRepository,
                                            ITaskManager taskManager)
    {
        this.dbFactory = dbFactory;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.fullCharacteristicRepository = fullCharacteristicRepository;
        this.taskManager = taskManager;
    }

    /// <summary>
    /// Calculates local characteristics for given subsequence.
    /// </summary>
    /// <param name="subsequenceId">
    /// The subsequence identifier.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic type link id.
    /// </param>
    /// <param name="windowSize">
    /// The sliding window size.
    /// </param>
    /// <param name="step">
    /// The sliding window shift.
    /// </param>
    /// <returns>
    /// Json containing local characteristics as <see cref="T:double[]"/>..
    /// </returns>
    [HttpGet]
    public ActionResult<double[]> GetSubsequenceCharacteristic(
        long subsequenceId,
        short characteristicLinkId,
        int windowSize,
        int step)
    {
        using var db = dbFactory.CreateDbContext();
        using var sequenceRepository = sequenceRepositoryFactory.Create();
        var calculator = new LocalCharacteristicsCalculator(db, fullCharacteristicRepository, sequenceRepository);
        double[] characteristics = calculator.GetSubsequenceCharacteristic(subsequenceId, characteristicLinkId, windowSize, step);

        return characteristics;
    }

    /// <summary>
    /// Calculates similarity matrix for the given local characteristics task.
    /// </summary>
    /// <param name="taskId">
    /// The task identifier.
    /// </param>
    /// <param name="aligner">
    /// The aligner.
    /// </param>
    /// <param name="distanceCalculator">
    /// The distance calculator.
    /// </param>
    /// <param name="aggregator">
    /// The aggregator.
    /// </param>
    /// <returns>
    /// Json containing similarity matrix as <see cref="T:double[,]"/>.
    /// </returns>
    [HttpGet]
    public ActionResult<Dictionary<string, object>> CalculateLocalCharacteristicsSimilarityMatrix(
        int taskId,
        Aligner aligner,
        DistanceCalculator distanceCalculator,
        Aggregator aggregator)
    {
        string data = taskManager.GetTaskData(taskId);

        var characteristicsObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);
        var characteristics = characteristicsObject["characteristics"];
        LocalCharacteristicsData[] chars = characteristics.ToObject<LocalCharacteristicsData[]>();

        double[][] series = new double[chars.Length][];

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

        // Using array of arrays instead of 2d array 
        // because json serializer does not support
        // multidimentional arrays serialization
        // TODO: try to fix this
        double[][] result = new double[series.Length][];
        for (int i = 0; i < series.Length; i++)
        {
            result[i] = new double[series.Length];
        }

        for (int i = 0; i < series.Length - 1; i++)
        {
            double[] firstSeries = series[i];
            for (int j = i + 1; j < series.Length; j++)
            {
                double[] secondSeries = series[j];
                result[i][j] = comparer.GetDistance(firstSeries, secondSeries);
                result[j][i] = result[i][j];
            }
        }

        return new Dictionary<string, object>
        {
            { "aligner", aligner },
            { "distanceCalculator", distanceCalculator },
            { "aggregator", aggregator },
            { "result", result }
        };
    }
}
