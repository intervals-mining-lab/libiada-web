namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;

using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.SequenceGenerator;

using Libiada.Web.Tasks;

/// <summary>
/// Calculates distribution of sequences by order.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequencesOrderDistributionController : AbstractResultController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SequencesOrderDistributionController"/> class.
    /// </summary>
    public SequencesOrderDistributionController(ITaskManager taskManager) : base(TaskType.SequencesOrderDistribution, taskManager)
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
    /// <param name="typeGenerate">
    /// Sequence generation type.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(int length, int alphabetCardinality, int typeGenerate)
    {
        return CreateTask(() =>
        {
            ISequenceGenerator sequenceGenerator;
            var orderGenerator = new OrderGenerator();
            List<int[]> orders;
            // TODO: add enum
            switch (typeGenerate)
            {
                case 0:
                    sequenceGenerator = new StrictSequenceGenerator();
                    orders = orderGenerator.StrictGenerateOrders(length, alphabetCardinality);
                    break;
                case 1:
                    sequenceGenerator = new SequenceGenerator();
                    orders = orderGenerator.GenerateOrders(length, alphabetCardinality);
                    break;
                case 2:
                    sequenceGenerator = new NonRedundantStrictSequenceGenerator();
                    orders = orderGenerator.StrictGenerateOrders(length, alphabetCardinality);
                    break;
                case 3:
                    sequenceGenerator = new NonRedundantSequenceGenerator();
                    orders = orderGenerator.GenerateOrders(length, alphabetCardinality);
                    break;
                default: throw new ArgumentException("Invalid type of generate");
            }
            List<BaseChain> sequences = sequenceGenerator.GenerateSequences(length, alphabetCardinality);
            var SequecesOrdersDistribution = new Dictionary<int[], List<BaseChain>>(new OrderEqualityComparer());
            foreach (int[] order in orders)
            {
                SequecesOrdersDistribution.Add(order, []);
            }

            foreach (BaseChain sequence in sequences)
            {
                SequecesOrdersDistribution[sequence.Order].Add(sequence);
            }

            var result = new Dictionary<string, object>
            {
                { 
                    "result", SequecesOrdersDistribution.Select(r => new
                    {
                        order = r.Key,
                        sequences = r.Value.Select(s => s.ToString(",")).ToArray()
                    }) 
                }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
