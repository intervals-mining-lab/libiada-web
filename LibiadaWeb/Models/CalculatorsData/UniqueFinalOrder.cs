using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibiadaWeb.Models.CalculatorsData
{
    public class UniqueFinalOrder
    {
        public int OrderId { get; set; }

        public int ParentOrderId { get; set; }

        public int Iteration { get; set; }

        public string Transformation { get; set; }
    }
}