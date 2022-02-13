namespace LibiadaWeb.Models.CalculatorsData
{
    using System;

    /// <summary>
    /// The attribute value.
    /// </summary>
    public readonly struct AttributeValue : IEquatable<AttributeValue>
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
        /// Initializes a new instance of the <see cref="AttributeValue"/> structure.
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
        public static bool operator ==(AttributeValue first, AttributeValue second) => first.AttributeId == second.AttributeId && first.Value == second.Value;

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
        public static bool operator !=(AttributeValue first, AttributeValue second) => !(first == second);

        /// <summary>
        /// Compares this attribute id and value to another.
        /// </summary>
        /// <param name="other">
        /// Another <see cref="AttributeValue"/>.
        /// </param>
        /// <returns></returns>
        public bool Equals(AttributeValue other) => this == other;

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// Attribute value to compare to.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object other) => other is AttributeValue attributeValue && this == attributeValue;

        /// <summary>
        /// Calculates hash using <see cref="AttributeId"/> and <see cref="Value"/> hash codes.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1468198044;
                hashCode = (hashCode * -1521134295) + AttributeId.GetHashCode();
                hashCode = (hashCode * -1521134295) + Value.GetHashCode();
                return hashCode;
            }
        }
    }
}
