namespace LibiadaWeb.Maintenance
{
    using System;
    using AutoMapper;

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
        public DateTimeOffset? Created;

        /// <summary>
        /// The started.
        /// </summary>
        public DateTimeOffset? Started;

        /// <summary>
        /// The completed.
        /// </summary>
        public DateTimeOffset? Completed;

        /// <summary>
        /// The completed.
        /// </summary>
        public TimeSpan? ExecutionTime;

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
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="TaskData"/>.
        /// </returns>
        public TaskData Clone()
        {
            return Mapper.Map<TaskData, TaskData>(this);
        }
    }
}
