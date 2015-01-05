namespace LibiadaWeb.Maintenance
{
    using System;

    /// <summary>
    /// The task data.
    /// </summary>
    public class TaskData
    {
        /// <summary>
        /// The id.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The display name.
        /// </summary>
        public readonly string DisplayName;

        /// <summary>
        /// The task state.
        /// </summary>
        public TaskState TaskState;

        /// <summary>
        /// The created.
        /// </summary>
        public DateTime Created;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskData"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        public TaskData(int id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
            Created = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskData"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        /// <param name="taskState">
        /// The task state.
        /// </param>
        /// <param name="created">
        /// The created.
        /// </param>
        private TaskData(int id, string displayName, TaskState taskState, DateTime created)
        {
            Id = id;
            Created = created;
            TaskState = taskState;
            DisplayName = displayName;
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="TaskData"/>.
        /// </returns>
        public TaskData Clone()
        {
            return new TaskData(Id, DisplayName, TaskState, Created);
        }
    }
}