namespace LibiadaWeb.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// Tasks events for signalR messages.
    /// </summary>
    public enum TaskEvent
    {
        /// <summary>
        /// Add task event flag.
        /// </summary>
        addTask,

        /// <summary>
        /// Delete task event flag.
        /// </summary>
        deleteTask,

        /// <summary>
        /// Task statu change event flag.
        /// </summary>
        changeStatus
    }
}
