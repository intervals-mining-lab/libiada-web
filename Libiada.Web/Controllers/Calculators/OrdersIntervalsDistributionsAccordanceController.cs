namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Extensions;

using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.SequenceGenerator;

using Libiada.Web.Tasks;

/// <summary>
/// Calculates accordance of orders to intervals distributions.
/// </summary>
[Authorize(Roles = "Admin")]
public class OrdersIntervalsDistributionsAccordanceController : AbstractResultController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersIntervalsDistributionsAccordanceController"/> class.
    /// </summary>
    public OrdersIntervalsDistributionsAccordanceController(ITaskManager taskManager) : base(TaskType.OrdersIntervalsDistributionsAccordance, taskManager)
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
    public ActionResult Index(int length, int alphabetCardinality, int generateStrict)
    {
        return CreateTask(() =>
        {
            var orderGenerator = new OrderGenerator();
            List<int[]> orders = generateStrict switch
            {
                0 => orderGenerator.StrictGenerateOrders(length, alphabetCardinality),
                1 => orderGenerator.GenerateOrders(length, alphabetCardinality),
                _ => throw new ArgumentException("Invalid type of generate"),
            };
            var distributionsAccordance = new Dictionary<string, Dictionary<Dictionary<int, int>, List<int[]>>>();
            foreach (Link link in EnumExtensions.ToArray<Link>())
            {
                if (link == Link.NotApplied)
                {
                    continue;
                }
                var accordance = new Dictionary<Dictionary<int, int>, List<int[]>>();
                foreach (int[] order in orders)
                {
                    var sequence = new ComposedSequence(order.Select(Convert.ToInt16).ToArray());
                    var fullIntervals = new Dictionary<int, int>();
                    var alphabet = sequence.Alphabet.ToList();
                    foreach (IBaseObject el in alphabet)
                    {
                        int[] congIntervals = sequence.CongenericSequence(el).GetArrangement(link);
                        foreach (int interval in congIntervals)
                        {
                            if (fullIntervals.Any(e => e.Key == interval))
                            {
                                fullIntervals[interval]++;
                            }
                            else
                            {
                                fullIntervals.Add(interval, 1);
                            }
                        }
                    }
                    if (accordance.Keys.Any(intervals => intervals.All(i1 => fullIntervals.Any(i2 => i2.Key == i1.Key && i2.Value == i1.Value))))
                    {
                        accordance[accordance.Keys.First(intervals => intervals.All(i1 => fullIntervals.Any(i2 => i2.Key == i1.Key && i2.Value == i1.Value)))].Add(order);
                    }
                    else
                    {
                        accordance.Add(fullIntervals, [order]);
                    }
                }
                
                distributionsAccordance.Add(link.GetDisplayValue(), accordance);
            }
            
            var linksList = Extensions.EnumExtensions.GetSelectList<Link>().ToList();
            linksList.RemoveAt(0);
            
            var result = new Dictionary<string, object>
            {
                { "result", distributionsAccordance.Select(r => new
                    {
                        link = r.Key,
                        accordance = r.Value.Select(d => new {
                            distributionIntervals = d.Key.Select(pair => new
                            {
                                interval = pair.Key,
                                count = pair.Value
                            }).ToArray(),
                            orders = d.Value.ToArray()
                        })
                    })
                },
                { "linkList", linksList }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
