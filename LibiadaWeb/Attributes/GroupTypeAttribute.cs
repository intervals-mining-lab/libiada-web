namespace LibiadaWeb.Attributes
{
    using System;

    /// <summary>
    /// The group type attribute.
    /// Used to specify group types hierarchy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GroupTypeAttribute : System.Attribute
    {
        /// <summary>
        /// The group type value.
        /// </summary>
        public readonly GroupType Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeAttribute"/> class.
        /// </summary>
        /// <param name="value">
        /// The group type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if value is not a valid <see cref="GroupType"/>.
        /// </exception>
        public GroupTypeAttribute(GroupType value)
        {
            if (!Enum.IsDefined(typeof(GroupType), value))
            {
                throw new ArgumentException("GroupType attribute value is not valid group type", nameof(value));
            }

            Value = value;
        }
    }
}