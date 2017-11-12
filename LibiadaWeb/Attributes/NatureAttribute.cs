namespace LibiadaWeb.Attributes
{
    using System;

    /// <summary>
    /// The nature attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NatureAttribute : System.Attribute
    {
        /// <summary>
        /// Nature attribute value.
        /// </summary>
        public readonly Nature Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatureAttribute"/> class.
        /// </summary>
        /// <param name="value">
        /// The Nature value.
        /// </param>
        public NatureAttribute(Nature value)
        {
            if (!Enum.IsDefined(typeof(Nature), value))
            {
                throw new ArgumentException("Nature attribute value is not valid nature", nameof(value));
            }

            Value = value;
        }
    }
}
