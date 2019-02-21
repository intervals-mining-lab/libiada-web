using System;
using System.Threading.Tasks;
using LibiadaCore.Misc;
using LibiadaWeb.Models.CalculatorsData;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using LibiadaCore.Core;
using SequenceGenerator;

namespace LibiadaWeb.Models
{
    public class OrderTransformer
    {
        private OrderGenerator orderGenerator;

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

        public OrderTransformer()
        {
            orderGenerator = new OrderGenerator();
        }

        public void CalculateTransformations(int length)
        {
            orders = orderGenerator.GenerateOrders(length);
            ordersCount = orders.Count;
            transformationsData = new OrderTransformationData[ordersCount];
        }

        private void TransformOrders()
        {
            var resultData = new List<OrderTransformationData>();
            var ordersIds = orders.Select(o => orders.IndexOf(o)).ToArray();
            var typesIds = typesOfTransformations.Select(t => typesOfTransformations.IndexOf(t));
            transformationsData = ordersIds.AsParallel().AsOrdered().Select(el => new OrderTransformationData
            {
                ResultTransformation = typesIds.AsParallel().AsOrdered().Select(t => TransformOrder(t, el)).ToArray()
            }).ToArray();
            resultData = ordersIds.AsParallel().AsOrdered().Select(el => new OrderTransformationData
            {
                ResultTransformation = transformationsData[el].ResultTransformation,
                UniqueFinalOrdersCount = CalculateUniqueOrdersCount(el)
            }).ToList();
            transformationsData = resultData.ToArray();
        }

        private OrderTransformationResult TransformOrder(int transformationType, int id)
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

        private int CalculateUniqueOrdersCount(int id)
        {
            bool completed = false;
            var ordersForChecking = new List<int> { id };
            var checkedOrders = new List<int> { id };
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