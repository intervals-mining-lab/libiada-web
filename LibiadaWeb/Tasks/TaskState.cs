namespace LibiadaWeb.Tasks
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The task state.
    /// </summary>
    public enum TaskState : byte
    {
        /// <summary>
        /// The task is in queue.
        /// </summary>
        [Display(Name = "In queue")]
        [Description("The task is in queue")]
        InQueue = 1,

        /// <summary>
        /// The task is in progress.
        /// </summary>
        [Display(Name = "In progress")]
        [Description("The task is in progress")]
        InProgress = 2,

        /// <summary>
        /// The task is completed.
        /// </summary>
        [Display(Name = "Completed")]
        [Description("The task is completed")]
        Completed = 3,

        /// <summary>
        /// Error occurred.
        /// </summary>
        [Display(Name = "Error")]
        [Description("Error occurred")]
        Error = 4
    }
}
