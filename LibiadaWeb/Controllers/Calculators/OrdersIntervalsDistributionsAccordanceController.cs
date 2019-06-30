using System;

namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Extensions;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using SequenceGenerator;

    /// <summary>
    /// Calculates accordance of orders by intervals distributions.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrdersIntervalsDistributionsAccordanceController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersIntervalsDistributionsAccordanceController"/> class.
        /// </summary>
        public OrdersIntervalsDistributionsAccordanceController() : base(TaskType.OrdersIntervalsDistributionsAccordance)
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
        public ActionResult Index(int length, int alphabetCardinality, int generateStrict, Link link)
        {
            return CreateTask(() =>
            {
                var orderGenerator = new OrderGenerator();
                var orders = new List<int[]>();
                switch (generateStrict)
                {
                    case 0:
                        orders = orderGenerator.StrictGenerateOrders(length, alphabetCardinality);
                        break;
                    case 1:
                        orders = orderGenerator.GenerateOrders(length, alphabetCardinality);
                        break;
                    default: throw new ArgumentException("Invalid type of generate");
                }
                Dictionary<Dictionary<int, int>, List<int[]>> result = new Dictionary<Dictionary<int, int>, List<int[]>>();
                foreach(var order in orders)
                {
                    var sequence = new Chain(order.Select(Convert.ToInt16).ToArray());
                    sequence.FillIntervalManagers();
                    var fullIntervals = new Dictionary<int,int>();
                    foreach(var el in sequence.Alphabet.ToList())
                    {
                        var congIntervals = sequence.CongenericChain(el).GetArrangement(link);
                        foreach(var interval in congIntervals)
                        {
                            if(fullIntervals.Any(e => e.Key == interval))
                            {
                                fullIntervals[interval]++;
                            }
                            else
                            {
                                fullIntervals.Add(interval, 1);
                            }
                        }
                    }
                    if(result.Keys.Any(intervals => intervals.SequenceEqual(fullIntervals)))
                    {
                        result[result.Keys.First(intervals => intervals.SequenceEqual(fullIntervals))].Add(order);
                    }
                    else
                    {
                        result.Add(fullIntervals, new List<int[]> { order });
                    }
                }
                var data = new Dictionary<string, object>
                {
                    { "result", result.Select(r => new
                    {
                        distributionIntervals = r.Key.Select(pair => new
                        {
                            interval = pair.Key,
                            count = pair.Value
                        }).ToArray(),
                        orders = r.Value.ToArray()
                    })
                    },
                    { "link", EnumExtensions.GetName<Link>(link)}
                };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(data) }
                };
            });
        }
    }
}