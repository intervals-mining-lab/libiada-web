namespace LibiadaWeb.Attributes
{
    using System;

    using LibiadaWeb.Controllers;
    using LibiadaWeb.Tasks;

    /// <summary>
    /// Task class attribute.
    /// Used to link <see cref="TaskType"/> enum to task class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TaskClassAttribute : Attribute
    {
        /// <summary>
        /// Task class type.
        /// </summary>
        public readonly Type Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskClassAttribute"/> class.
        /// </summary>
        /// <param name="value">
        /// Task class type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if value is not derived from <see cref="AbstractResultController"/>
        /// </exception>
        public TaskClassAttribute(Type value)
        {
            if (!value.IsSubclassOf(typeof(AbstractResultController)))
            {
                throw new ArgumentException("Task class attribute value is invalid", nameof(value));
            }

            Value = value;
        }
    }
}