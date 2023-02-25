namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Iterators;
    using LibiadaCore.TimeSeries.Aggregators;
    using LibiadaCore.TimeSeries.Aligners;
    using LibiadaCore.TimeSeries.OneDimensional.Comparers;
    using LibiadaCore.TimeSeries.OneDimensional.DistanceCalculators;

    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The local calculation web api controller.
    /// </summary>
    [Authorize]
    public class LocalCalculationWebApiController : ApiController
    {
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
        public string GetSubsequenceCharacteristic(
            long subsequenceId,
            short characteristicLinkId,
            int windowSize,
            int step)
        {
            var calculator = new LocalCharacteristicsCalculator();
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
            var data = TaskManager.Instance.GetTaskData(taskId);

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
}
