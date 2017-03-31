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

        /// <summary>
        /// The == operator overload.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// True if both fields are equal and false otherwise.
        /// </returns>
        public static bool operator ==(AttributeValue first, AttributeValue second)
        {
            return first.AttributeId == second.AttributeId && first.Value.Equals(second.Value);
        }

        /// <summary>
        /// The != operator overload.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// False if both fields are equal and true otherwise.
        /// </returns>
        public static bool operator !=(AttributeValue first, AttributeValue second)
        {
            return !(first == second);
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is AttributeValue && this == (AttributeValue)obj;
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return AttributeId.GetHashCode() ^ Value.GetHashCode();
        }
    }
}
