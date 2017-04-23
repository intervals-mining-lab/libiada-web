namespace LibiadaWeb.Tasks
{
    /// <summary>
    /// Tasks events for signalR messages.
    /// </summary>
    public enum TaskEvent
    {
        /// <summary>
        /// Add task event flag.
        /// </summary>
        AddTask,

        /// <summary>
        /// Delete task event flag.
        /// </summary>
        DeleteTask,

        /// <summary>
        /// Task status change event flag.
        /// </summary>
        ChangeStatus
    }
}
