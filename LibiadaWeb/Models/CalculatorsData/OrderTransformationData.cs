using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibiadaWeb.Models.CalculatorsData
{
    public class OrderTransformationData
    {
        public int OrderId { get; set; }

        public OrderTransformationResult[] ResultTransformation { get; set; }

        public UniqueFinalOrder[] UniqueFinalOrders { get; set; }
    }
}