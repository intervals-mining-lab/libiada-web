using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibiadaWeb.Models.CalculatorsData
{
    public struct AttributeValue
    {
        public readonly byte AttributeId;

        public readonly string Value;

        public AttributeValue(byte attributeId, string value)
        {
            AttributeId = attributeId;
            Value = value;
        }
    }
}