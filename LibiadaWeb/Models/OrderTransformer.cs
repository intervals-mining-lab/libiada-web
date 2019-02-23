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
    using LibiadaWeb.Extensions;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    public class OrderTransformer
    {
        private OrderGenerator orderGenerator;

        private OrderTransformation[] typesOfTransformations;

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
            get { return typesOfTransformations.Select(t => EnumExtensions.GetDisplayValue(t)).ToArray(); }
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
            typesOfTransformations = EnumExtensions.ToArray<OrderTransformation>();
            TransformOrders();
        }

        private void TransformOrders()
        {
            var resultData = new List<OrderTransformationData>();
            var ordersIds = orders.Select(o => orders.IndexOf(o)).ToArray();
            transformationsData = ordersIds.AsParallel().AsOrdered().Select(el => new OrderTransformationData
            {
                ResultTransformation = typesOfTransformations.AsParallel().AsOrdered().Select(t => TransformOrder(t, el)).ToArray()
            }).ToArray();
            resultData = ordersIds.AsParallel().AsOrdered().Select(el => new OrderTransformationData
            {
                ResultTransformation = transformationsData[el].ResultTransformation,
                UniqueFinalOrdersCount = CalculateUniqueOrdersCount(el)
            }).ToList();
            transformationsData = resultData.ToArray();
        }

        private OrderTransformationResult TransformOrder(OrderTransformation transformationType, int id)
        {
            var transformationResult = new OrderTransformationResult();
            Chain chain = transformationType == OrderTransformation.Dissimilar
                              ? DissimilarChainFactory.Create(new BaseChain(orders[id]))
                              : HighOrderFactory.Create(
                                  new Chain(orders[id]),
                                  EnumExtensions.GetLink(transformationType));
            for (int i = 0; i < ordersCount; i++)
            {
                if (orders[i].SequenceEqual(chain.Building))
                {
                    transformationResult.OrderId = i;
                    transformationResult.Transformation = TypesOfTransformations[typesOfTransformations.IndexOf(transformationType)];
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