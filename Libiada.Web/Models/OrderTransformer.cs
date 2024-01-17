namespace Libiada.Web.Models;

using Libiada.SequenceGenerator;

using Libiada.Core.Core;
using Libiada.Core.DataTransformers;
using Libiada.Core.Extensions;

using Libiada.Database.Models.CalculatorsData;

using EnumExtensions = Libiada.Core.Extensions.EnumExtensions;

public class OrderTransformer
{
    public List<int[]> Orders { get; private set; }
    public OrderTransformationData[] TransformationsData { get; private set; }


    public void CalculateTransformations(int length)
    {
        var orderGenerator = new OrderGenerator();
        Orders = orderGenerator.GenerateOrders(length);
        TransformationsData = new OrderTransformationData[Orders.Count];
        TransformOrders();
    }

    private void TransformOrders()
    {
        int[] ordersIds = Enumerable.Range(0, Orders.Count).ToArray();
        TransformationsData = ordersIds.AsParallel().AsOrdered().Select(orderId => new OrderTransformationData
        {
            ResultTransformation = EnumExtensions.ToArray<OrderTransformation>().AsParallel().AsOrdered().Select(t => TransformOrder(t, orderId)).ToArray()
        }).ToArray();

        ordersIds.AsParallel().ForAll(orderId => TransformationsData[orderId].UniqueFinalOrdersCount = CalculateUniqueOrdersCount(orderId));
    }

    private OrderTransformationResult TransformOrder(OrderTransformation transformationType, int id)
    {
        var transformationResult = new OrderTransformationResult();
        Chain chain = transformationType == OrderTransformation.Dissimilar
                          ? DissimilarChainFactory.Create(new BaseChain(Orders[id]))
                          : HighOrderFactory.Create(new Chain(Orders[id]), transformationType.GetLink());

        for (int i = 0; i < Orders.Count; i++)
        {
            if (Orders[i].SequenceEqual(chain.Order))
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
        OrderTransformation[] transformationTypes = EnumExtensions.ToArray<OrderTransformation>();
        bool completed = false;
        var ordersForChecking = new List<int> { id };
        var checkedOrders = new List<int> { id };
        while (!completed)
        {
            var newOrders = new List<int>();
            foreach (int order in ordersForChecking)
            {
                for (int i = 0; i < transformationTypes.Length; i++)
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
