﻿namespace LibiadaWeb.Tasks
{
    /// <summary>
    /// The task state.
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// The task is in queue.
        /// </summary>
        InQueue,

        /// <summary>
        /// The task is in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The task is completed.
        /// </summary>
        Completed,

        /// <summary>
        /// Error occurred.
        /// </summary>
        Error
    }
}