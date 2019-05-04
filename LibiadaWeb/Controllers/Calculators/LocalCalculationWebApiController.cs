namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using Accord.IO;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Iterators;
    using LibiadaCore.TimeSeries.Aggregators;
    using LibiadaCore.TimeSeries.Aligners;
    using LibiadaCore.TimeSeries.OneDimensional.DistanceCalculators;

    using LibiadaWeb.Models;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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

            using (var context = new LibiadaWebEntities())
            {
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;

                FullCharacteristic characteristic =
                    characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
                link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

                var subsequenceExtractor = new SubsequenceExtractor(context);

                Subsequence subsequence = context.Subsequence.Single(s => s.Id == subsequenceId);

                chain = subsequenceExtractor.ExtractChains(subsequence.SequenceId, new[] { subsequence }).Single();
            }

            CutRule cutRule = new SimpleCutRule(chain.Length, step, windowSize);

            CutRuleIterator iterator = cutRule.GetIterator();

            var fragments = new List<Chain>();

            while (iterator.Next())
            {
                var fragment = new Chain(iterator.GetEndPosition() - iterator.GetStartPosition());

                for (int k = 0; iterator.GetStartPosition() + k < iterator.GetEndPosition(); k++)
                {
                    fragment.Set(chain[iterator.GetStartPosition() + k], k);
                }

                fragments.Add(fragment);
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


            //var characteristicsJson = JsonConvert.SerializeObject(data.Values);
            var characteristicsObject = JsonConvert.DeserializeObject<dynamic>(data);
            var characteristics = characteristicsObject.characteristics;
            LocalCharacteristicsData[] chars = characteristics.ToObject<LocalCharacteristicsData[]>();
            //LocalCharacteristicsData[] chars =
            //    JsonConvert.DeserializeObject<LocalCharacteristicsData[]>(characteristics);
            //LocalCharacteristicsData chars = (LocalCharacteristicsData)characteristics;
            //LocalCharacteristicsData chars = JsonConvert.DeserializeObject<LocalCharacteristicsData>((string)characteristics);
            //var chars = characteristicsObject;
            //characteristicsObject


            return "";
        }
    }
}
