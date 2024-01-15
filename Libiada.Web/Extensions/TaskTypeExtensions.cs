namespace Libiada.Web.Extensions;

using System;

using Libiada.Core.Extensions;

using Libiada.Web.Attributes;
using Libiada.Database.Tasks;

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