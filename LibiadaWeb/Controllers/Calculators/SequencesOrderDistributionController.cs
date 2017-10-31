using System.Collections.Generic;
using System.Web.Mvc;
using LibiadaCore.Core;
using LibiadaWeb.Tasks;
using Newtonsoft.Json;
using SequenceGenerator;

namespace LibiadaWeb.Controllers.Calculators
{
    /// <summary>
    /// Calculates distribution of sequences by order.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequencesOrderDistributionController : AbstractResultController
    {
        public SequencesOrderDistributionController() : base(TaskType.SequencesOrderDistribution)
        {
        }

        // GET: SequencesOrderDestribution
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(int length, int alphabetCardinality, bool generateStrict)
        {
            return CreateTask(() =>
            {
                var sequnceGenerator = new SequenceGenerator.SequenceGenerator();
                var orderGenerator = new OrderGenerator();
                List<BaseChain> sequnces;
                List<int[]> orders;
                if (generateStrict)
                {
                    sequnces = sequnceGenerator.StrictGenerateSequences(length, alphabetCardinality);
                    orders = orderGenerator.StrictGenerateOrders(length, alphabetCardinality);
                }
                else
                {
                    sequnces = sequnceGenerator.GenerateSequences(length, alphabetCardinality);
                    orders = orderGenerator.GenerateOrders(length, alphabetCardinality);
                }
                Dictionary<int[], List<BaseChain>> result = new Dictionary<int[], List<BaseChain>>();
                foreach (var order in orders)
                {
                    result.Add(order, new List<BaseChain>());
                }
                foreach (var sequence in sequnces)
                {
                    result[sequence.Building].Add(sequence);
                }
                return new Dictionary<string, object>
                {
                    {"data", JsonConvert.SerializeObject(result)}
                };
            });
        }
    }
}