namespace LibiadaWeb.Tasks
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The task state.
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// The task is in queue.
        /// </summary>
        [Display(Name = "In queue")]
        InQueue,

        /// <summary>
        /// The task is in progress.
        /// </summary>
        [Display(Name = "In progress")]
        InProgress,

        /// <summary>
        /// The task is completed.
        /// </summary>
        [Display(Name = "Completed")]
        Completed,

        /// <summary>
        /// Error occurred.
        /// </summary>
        [Display(Name = "Error")]
        Error
    }
}
