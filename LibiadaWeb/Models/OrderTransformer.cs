using LibiadaCore.Misc;
using LibiadaWeb.Models.CalculatorsData;
using System.Collections.Generic;
using System.Linq;
using LibiadaCore.Core;
using SequenceGenerator;

namespace LibiadaWeb.Models
{
    using LibiadaCore.Extensions;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    public class OrderTransformer
    {
        private readonly OrderGenerator orderGenerator;

        private OrderTransformation[] typesOfTransformations;
        private int ordersCount;

        public List<int[]> Orders { get; private set; }
        public OrderTransformationData[] TransformationsData { get; private set; }

        public OrderTransformer()
        {
            orderGenerator = new OrderGenerator();
        }

        public void CalculateTransformations(int length)
        {
            Orders = orderGenerator.GenerateOrders(length);
            ordersCount = Orders.Count;
            TransformationsData = new OrderTransformationData[ordersCount];
            typesOfTransformations = EnumExtensions.ToArray<OrderTransformation>();
            TransformOrders();
        }

        private void TransformOrders()
        {
            int[] ordersIds = Orders.Select(o => Orders.IndexOf(o)).ToArray();
            TransformationsData = ordersIds.AsParallel().AsOrdered().Select(el => new OrderTransformationData
            {
                ResultTransformation = typesOfTransformations.AsParallel().AsOrdered().Select(t => TransformOrder(t, el)).ToArray()
            }).ToArray();

            List<OrderTransformationData> resultData = ordersIds.AsParallel().AsOrdered().Select(el => new OrderTransformationData
            {
                ResultTransformation = TransformationsData[el].ResultTransformation,
                UniqueFinalOrdersCount = CalculateUniqueOrdersCount(el)
            }).ToList();

            TransformationsData = resultData.ToArray();
        }

        private OrderTransformationResult TransformOrder(OrderTransformation transformationType, int id)
        {
            var transformationResult = new OrderTransformationResult();
            Chain chain = transformationType == OrderTransformation.Dissimilar
                              ? DissimilarChainFactory.Create(new BaseChain(Orders[id]))
                              : HighOrderFactory.Create(new Chain(Orders[id]), transformationType.GetLink());

            for (int i = 0; i < ordersCount; i++)
            {
                if (Orders[i].SequenceEqual(chain.Building))
                {
                    transformationResult.OrderId = i;
                    transformationResult.Transformation = transformationType.GetDisplayValue();
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
                foreach (int order in ordersForChecking)
                {
                    for (int i = 0; i < typesOfTransformations.Length; i++)
                    {
                        if (!checkedOrders.Contains(TransformationsData[order].ResultTransformation[i].OrderId))
                        {
                            checkedOrders.Add(TransformationsData[order].ResultTransformation[i].OrderId);
                            newOrders.Add(TransformationsData[order].ResultTransformation[i].OrderId);
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