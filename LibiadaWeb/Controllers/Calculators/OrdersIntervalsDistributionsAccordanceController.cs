﻿using System;

namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
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
        public ActionResult Index(int length, int alphabetCardinality, int generateStrict)
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
                var result = new Dictionary<string, Dictionary<Dictionary<int, int>, List<int[]>>>();
                foreach (var link in EnumExtensions.ToArray<Link>())
                {
                    if (link == Link.NotApplied)
                    {
                        continue;
                    }
                    var accordance = new Dictionary<Dictionary<int, int>, List<int[]>>();
                    foreach (var order in orders)
                    {
                        var sequence = new Chain(order.Select(Convert.ToInt16).ToArray());
                        var fullIntervals = new Dictionary<int, int>();
                        foreach (var el in sequence.Alphabet.ToList())
                        {
                            var congIntervals = sequence.CongenericChain(el).GetArrangement(link);
                            foreach (var interval in congIntervals)
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
                            accordance.Add(fullIntervals, new List<int[]> { order });
                        }
                    }
                    result.Add(EnumExtensions.GetDisplayValue<Link>(link), accordance);
                }
                var list = EnumHelper.GetSelectList(typeof(Link));
                list.RemoveAt(0);
                var data = new Dictionary<string, object>
                {
                    { "result", result.Select(r => new
                    {
                        link = r.Key.ToString(),
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
                    {"linkList",list }
                };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(data) }
                };
            });
        }
    }
}