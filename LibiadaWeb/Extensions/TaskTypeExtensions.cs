namespace LibiadaWeb.Extensions
{
    using System;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Attributes;
    using LibiadaWeb.Tasks;

    public static class TaskTypeExtensions
    {
        /// <summary>
        /// Gets task class (Type) for given <see cref="TaskType"/> enum value.
        /// </summary>
        /// <param name="value">
        /// The <see cref="TaskType"/> enum value.
        /// </param>
        /// <returns>
        /// Task class as <see cref="Type"/>.
        /// </returns>
        public static Type GetTaskClass(this TaskType value)
        {
            return value.GetAttribute<TaskType, TaskClassAttribute>().Value;
        }
    }
}