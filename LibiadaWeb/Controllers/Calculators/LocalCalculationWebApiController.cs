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
            Chain chain;
            IFullCalculator calculator;
            Link link;

            using (var db = new LibiadaWebEntities())
            {
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;

                FullCharacteristic characteristic =
                    characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
                link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

                var subsequenceExtractor = new SubsequenceExtractor(db);

                Subsequence subsequence = db.Subsequence.Single(s => s.Id == subsequenceId);
                chain = subsequenceExtractor.GetSubsequenceSequence(subsequence);
            }

            CutRule cutRule = new SimpleCutRule(chain.Length, step, windowSize);

            CutRuleIterator iterator = cutRule.GetIterator();

            var fragments = new List<Chain>();

            while (iterator.Next())
            {
                int start = iterator.GetStartPosition();
                int end = iterator.GetEndPosition();

                var fragment = new List<IBaseObject>();
                for (int k = 0; start + k < end; k++)
                {
                    fragment.Add(chain[start + k]);
                }

                fragments.Add(new Chain(fragment));
            }

            var characteristics = new double[fragments.Count];

            for (int k = 0; k < fragments.Count; k++)
            {
                characteristics[k] = calculator.Calculate(fragments[k], link);
            }

            return JsonConvert.SerializeObject(characteristics);
        }

        [HttpGet]
        public string CalculateLocalCharacteristicsSimilarityMatrix(
            int taskId,
            Aligner aligner,
            DistanceCalculator distanceCalculator,
            Aggregator aggregator)
        {
            TaskManager taskManager = TaskManager.Instance;
            Task task = taskManager.GetTask(taskId);

            var data = (string)task.Result["data"];

            var characteristicsObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);
            var characteristics = characteristicsObject["characteristics"];
            LocalCharacteristicsData[] chars = characteristics.ToObject<LocalCharacteristicsData[]>();

            var series = new double[chars.Length][];

            for (int i = 0; i < chars.Length; i++)
            {
                series[i] = chars[i].fragmentsData.Select(fd => fd.Characteristics[0]).ToArray();
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
                for (int j = i + 1; j < series.Length; j++)
                {
                    result[i,j] = comparer.GetDistance(series[i], series[j]);
                    result[j,i] = comparer.GetDistance(series[i], series[j]);
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
