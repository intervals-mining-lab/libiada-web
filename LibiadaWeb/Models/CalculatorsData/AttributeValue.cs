namespace LibiadaWeb.Models.CalculatorsData
{
    /// <summary>
    /// The attribute value.
    /// </summary>
    public struct AttributeValue
    {
        /// <summary>
        /// The attribute id.
        /// </summary>
        public readonly byte AttributeId;

        /// <summary>
        /// Attribute value.
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValue"/> struct.
        /// </summary>
        /// <param name="attributeId">
        /// The attribute id.
        /// </param>
        /// <param name="value">
        /// The attribute value.
        /// </param>
        public AttributeValue(byte attributeId, string value)
        {
            AttributeId = attributeId;
            Value = value;
        }
    }
}
