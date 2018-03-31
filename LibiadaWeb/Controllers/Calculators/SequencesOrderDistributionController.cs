namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using SequenceGenerator;

    /// <summary>
    /// Calculates distribution of sequences by order.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequencesOrderDistributionController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesOrderDistributionController"/> class.
        /// </summary>
        public SequencesOrderDistributionController() : base(TaskType.SequencesOrderDistribution)
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.data = "{}";
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="alphabetCardinality">
        /// The alphabet cardinality.
        /// </param>
        /// <param name="generateStrict">
        /// The generate strict.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(int length, int alphabetCardinality, bool generateStrict)
        {
            return CreateTask(() =>
            {
                var sequenceGenerator = generateStrict ?
                                       (ISequenceGenerator)new StrictSequenceGenerator() :
                                       new SequenceGenerator();
                var sequences = sequenceGenerator.GenerateSequences(length, alphabetCardinality);
                var orderGenerator = new OrderGenerator();
                var orders = generateStrict ?
                                 orderGenerator.StrictGenerateOrders(length, alphabetCardinality) :
                                 orderGenerator.GenerateOrders(length, alphabetCardinality);
                Dictionary<int[], List<BaseChain>> result = new Dictionary<int[], List<BaseChain>>(new OrderEqualityComparer());
                foreach (var order in orders)
                {
                    result.Add(order, new List<BaseChain>());
                }

                foreach (var sequence in sequences)
                {
                    result[sequence.Building].Add(sequence);
                }

                var data = new Dictionary<string, object>
                {
                    { "result", result.Select(r => new { order = r.Key, sequences = r.Value.Select(s => s.ToString(",")).ToArray() }) }
                };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(data) }
                };
            });
        }
    }
}