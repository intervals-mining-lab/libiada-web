using System;
using System.Threading.Tasks;
using System.Web.Services.Description;
using LibiadaCore.Misc;
using LibiadaWeb.Models.CalculatorsData;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using LibiadaCore.Core;

using LibiadaWeb.Tasks;

using Newtonsoft.Json;

using SequenceGenerator;

namespace LibiadaWeb.Models
{
    public class OrderTransformer
    {
        private string[] typesOfTransformations = new string[]{
            "Dissimilar order",
            "High order, link start",
            "High order, link end",
            "High order, link cycle start",
            "High order, link cycle end"
        };

        private List<int[]> orders;
        private OrderTransformationData[] transformationsData;
        private int ordersCount;

        public List<int[]> Orders
        {
            get { return orders; }
        }
        public OrderTransformationData[] TransformationsData
        {
            get { return transformationsData; }
        }
        public string[] TypesOfTransformations
        {
            get { return typesOfTransformations; }
        }

        public OrderTransformer(List<int[]> orders)
        {
            this.orders = orders;
            transformationsData = new OrderTransformationData[ordersCount];
            ordersCount = orders.Count;
        }

        public void TransformateOrders(bool multiThreading)
        {
            var resultData = new List<OrderTransformationData>();
            if (multiThreading)
            {
                var ordersTasks = new Task<OrderTransformationData>[ordersCount];
                var countTasks = new Task<int>[ordersCount];
                for (int i = 0; i < ordersCount; i++)
                {
                    int orderIndex = i;
                    ordersTasks[i] = new Task<OrderTransformationData>(() =>
                    {
                        var transformationsTasks = new Task<OrderTransformationResult>[typesOfTransformations.Length];
                        for (int j = 0; j < typesOfTransformations.Length; j++)
                        {
                            int typeOfTransformation = j;
                            transformationsTasks[j] = new Task<OrderTransformationResult>(() => TransformateOrder(typeOfTransformation, orderIndex));
                        }
                        foreach (var task in transformationsTasks)
                        {
                            task.Start();
                        }
                        System.Threading.Tasks.Task.WaitAll(transformationsTasks);
                        return new OrderTransformationData
                        {
                            ResultTransformation = transformationsTasks.Select(t => t.Result).ToArray()
                        };
                    });
                    countTasks[i] = new Task<int>(() => SearchUniqueFinalOrdersCount(orderIndex));
                }
                foreach (var task in ordersTasks)
                {
                    task.Start();
                }
                System.Threading.Tasks.Task.WaitAll(ordersTasks);
                foreach (var task in ordersTasks)
                {
                    resultData.Add(task.Result);
                }
                transformationsData = resultData.ToArray();
                foreach (var task in countTasks)
                {
                    task.Start();
                }
                System.Threading.Tasks.Task.WaitAll(countTasks);
                for (int i = 0; i < ordersCount; i++)
                {
                    resultData[i].UniqueFinalOrdersCount = countTasks[i].Result;
                }
            }
            else
            {
                for (int i = 0; i < ordersCount; i++)
                {
                    var transformationResults = new List<OrderTransformationResult>();
                    for (int j = 0; j < typesOfTransformations.Length; j++)
                    {
                        transformationResults.Add(TransformateOrder(j, i));
                    }
                    resultData.Add(new OrderTransformationData());
                    resultData[i].ResultTransformation = transformationResults.ToArray();
                }
                transformationsData = resultData.ToArray();
                for (int i = 0; i < ordersCount; i++)
                {
                    int count = SearchUniqueFinalOrdersCount(i);
                    resultData[i].UniqueFinalOrdersCount = count;
                }
            }
            transformationsData = resultData.ToArray();
        }

        private OrderTransformationResult TransformateOrder(int transformationType, int id)
        {
            var transformationResult = new OrderTransformationResult();
            Chain chain;
            switch (transformationType)
            {

                case 0: chain = DissimilarChainFactory.Create((BaseChain)new Chain(orders[id])); break;
                case 1: chain = HighOrderFactory.Create(new Chain(orders[id]), Link.Start); break;
                case 2: chain = HighOrderFactory.Create(new Chain(orders[id]), Link.End); break;
                case 3: chain = HighOrderFactory.Create(new Chain(orders[id]), Link.CycleEnd); break;
                case 4: chain = HighOrderFactory.Create(new Chain(orders[id]), Link.CycleStart); break;
                default: throw new ArgumentException("Invalid type transformation");
            }
            for (int i = 0; i < ordersCount; i++)
            {
                if (orders[i].SequenceEqual(chain.Building))
                {
                    transformationResult.OrderId = i;
                    transformationResult.Transformation =
                        typesOfTransformations[transformationType];
                    break;
                }
            }
            return transformationResult;
        }

        private int SearchUniqueFinalOrdersCount(int id)
        {
            bool completed = false;
            var ordersForChecking = new List<int> { id };
            var checkedOrders = new List<int> {id};
            while (!completed)
            {
                var newOrders = new List<int>();
                foreach (var order in ordersForChecking)
                {
                    for (int i = 0; i < typesOfTransformations.Length; i++)
                    {
                        if (!checkedOrders.Contains(transformationsData[order].ResultTransformation[i].OrderId))
                        {
                            checkedOrders.Add(transformationsData[order].ResultTransformation[i].OrderId);
                            newOrders.Add(transformationsData[order].ResultTransformation[i].OrderId);
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
            return checkedOrders.Count;
        }
    }
}