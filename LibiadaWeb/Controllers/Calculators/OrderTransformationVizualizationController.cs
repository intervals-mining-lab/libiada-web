using System;
using System.Threading.Tasks;
using System.Web.Services.Description;
using LibiadaCore.Misc;
using LibiadaWeb.Models.CalculatorsData;

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
    public class OrderTransformationVisualizationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformationVisualizationController"/> class.
        /// </summary>
        public OrderTransformationVisualizationController() : base(TaskType.OrderTransformationVisualization)
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
        public ActionResult Index(int length)
        {
            return CreateTask(() =>
            {
                string[] typesOfTransformations = new string[]{
                    "Dissimilar order",
                    "High order, link start",
                    "High order, link end",
                    "High order, link cycle start",
                    "High order, link cycle end"
                };
                
                var orderGenerator = new OrderGenerator();
                var orders = orderGenerator.GenerateOrders(length);
                var ordersData = new OrderData[orders.Count];
                for (int i = 0; i < ordersData.Length; i++)
                {
                    ordersData[i] = new OrderData()
                    {
                        OrderId = i + 1,
                        Order = orders[i]
                    };
                }
                orderGenerator = null;
                orders = null;
                var countOfOrders = ordersData.Length;
                var ordersTasks = new Task<OrderTransformationData>[countOfOrders];
                for (int i = 0; i < countOfOrders; i++)
                {
                    int orderIndex = i;
                    ordersTasks[i] = new Task<OrderTransformationData>(() =>
                    {
                        var transformationData = new OrderTransformationData();
                        var tasksOftransformations = new Task<OrderTransformationResult>[typesOfTransformations.Length];
                        for (byte j = 0; j < typesOfTransformations.Length; j++)
                        {
                            byte transformationType = j;
                            tasksOftransformations[transformationType] = new Task<OrderTransformationResult>(() =>
                            {
                                var transformationResult = new OrderTransformationResult();
                                Chain chain;
                                var order = ordersData[orderIndex].Order.Select(n => Convert.ToInt16(n)).ToArray();
                                switch (transformationType)
                                {
                                        
                                    case 0: chain = DissimilarChainFactory.Create((BaseChain)new Chain(order)); break;
                                    case 1: chain = HighOrderFactory.Create(new Chain(order), Link.Start); break;
                                    case 2: chain = HighOrderFactory.Create(new Chain(order), Link.End); break;
                                    case 3: chain = HighOrderFactory.Create(new Chain(order), Link.CycleEnd); break;
                                    case 4: chain = HighOrderFactory.Create(new Chain(order), Link.CycleStart); break;
                                    default: throw new ArgumentException("Invalid type transformation");
                                }
                                for (int k = 0; k < countOfOrders; k++)
                                {
                                    bool isEquel = true;
                                    for (int h = 0; h < length; h++)
                                    {
                                        if (ordersData[k].Order[h] != chain.Building[h])
                                        {
                                            isEquel = false;
                                            break;
                                        }
                                    }
                                    if (isEquel)
                                    {
                                        transformationResult.OrderId = k + 1;
                                        transformationResult.Transformation =
                                            typesOfTransformations[transformationType];
                                        break;
                                    }
                                }
                                return transformationResult;
                            });
                        }
                        foreach (var task in tasksOftransformations)
                        {
                            task.Start();
                        }
                        System.Threading.Tasks.Task.WaitAll(tasksOftransformations);
                        transformationData.OrderId = orderIndex + 1;
                        transformationData.ResultTransformation =
                            tasksOftransformations.Select(t => t.Result).ToArray();
                        return transformationData;
                    });
                }
                foreach (var task in ordersTasks)
                {
                    task.Start();
                }
                System.Threading.Tasks.Task.WaitAll(ordersTasks);
                var transformationsData = ordersTasks.Select(t => t.Result).ToArray();
                foreach (var transformationData in transformationsData)
                {
                    var uniqueFinalOrders = new List<UniqueFinalOrder>();
                    var ordersForChecking = new List<int>();
                    foreach (var resultTransformation in transformationData.ResultTransformation)
                    {
                        if (uniqueFinalOrders.All(o => o.OrderId != resultTransformation.OrderId))
                        {
                            uniqueFinalOrders.Add(new UniqueFinalOrder()
                            {
                                OrderId = resultTransformation.OrderId,
                                ParentOrderId = transformationData.OrderId,
                                Iteration = 1,
                                Transformation = resultTransformation.Transformation
                            });
                            ordersForChecking.Add(resultTransformation.OrderId);
                        }
                    }
                    bool completed = false;
                    for (int i = 2; !completed; i++)
                    {
                        var newOrders = new List<int>();
                        foreach (var order in ordersForChecking)
                        {
                            var orderTransformationData = transformationsData.First(d => d.OrderId == order);
                            foreach (var resultTransformation in orderTransformationData.ResultTransformation)
                            {
                                if (uniqueFinalOrders.All(o => o.OrderId != resultTransformation.OrderId))
                                {
                                    newOrders.Add(resultTransformation.OrderId);
                                    uniqueFinalOrders.Add(new UniqueFinalOrder()
                                    {
                                        OrderId = resultTransformation.OrderId,
                                        ParentOrderId = order,
                                        Iteration = i,
                                        Transformation = resultTransformation.Transformation
                                    });
                                }
                            }
                        }
                        if (newOrders.Count > 0)
                        {
                            ordersForChecking = newOrders;
                        }
                        else
                        {
                            completed = true;
                        }
                    }
                    transformationData.UniqueFinalOrders = uniqueFinalOrders.ToArray();
                }

                var typesOfTransformationsList = new SelectListItem[typesOfTransformations.Length+1];
                typesOfTransformationsList[0] = new SelectListItem
                {
                    Value = 0.ToString(),
                    Text = "All",
                    Selected = false
                };
                for (int i = 0; i < typesOfTransformations.Length; i++)
                {
                    typesOfTransformationsList[i+1] = new SelectListItem
                    {
                        Value = (i-1).ToString(),
                        Text = typesOfTransformations[i],
                        Selected = false
                    };
                }

                var data = new Dictionary<string, object>
                {
                    { "transformationsData", transformationsData},
                    { "orders", ordersData},
                    { "transformationsName", typesOfTransformations},
                    { "transformationsList", typesOfTransformationsList},

                };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(data) }
                };
            });
        }
    }
}