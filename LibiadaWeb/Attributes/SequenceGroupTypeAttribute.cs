namespace LibiadaWeb.Attributes
{
    using System;

    /// <summary>
    /// The group type attribute.
    /// Used to specify group types hierarchy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SequenceGroupTypeAttribute : System.Attribute
    {
        /// <summary>
        /// The group type value.
        /// </summary>
        public readonly SequenceGroupType Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceGroupTypeAttribute"/> class.
        /// </summary>
        /// <param name="value">
        /// The group type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if value is not a valid <see cref="SequenceGroupType"/>.
        /// </exception>
        public SequenceGroupTypeAttribute(SequenceGroupType value)
        {
            if (!Enum.IsDefined(typeof(SequenceGroupType), value))
            {
                throw new ArgumentException("GroupType attribute value is not valid group type", nameof(value));
            }

            Value = value;
        }
    }
}