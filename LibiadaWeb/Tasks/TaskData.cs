namespace LibiadaWeb.Tasks
{
    using System;
    using System.Web;

    using AutoMapper;

    /// <summary>
    /// The task data.
    /// </summary>
    public class TaskData
    {
        /// <summary>
        /// The task type.
        /// </summary>
        public readonly TaskType TaskType;

        /// <summary>
        /// The user id.
        /// </summary>
        public readonly int UserId;

        /// <summary>
        /// The user name.
        /// </summary>
        public readonly string UserName;

        /// <summary>
        /// The id.
        /// </summary>
        public readonly long Id;

        /// <summary>
        /// The task state.
        /// </summary>
        public TaskState TaskState;

        /// <summary>
        /// The created.
        /// </summary>
        public DateTimeOffset Created;

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
        /// <param name="userId">
        /// Creator id.
        /// </param>
        /// <param name="taskType">
        /// The task Type.
        /// </param>
        public TaskData(long id, int userId, TaskType taskType)
        {
            Id = id;
            TaskType = taskType;
            UserId = userId;
            UserName = HttpContext.Current.User.Identity.Name;
            Created = DateTime.Now;
            TaskState = TaskState.InQueue;
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="TaskData"/>.
        /// </returns>
        public TaskData Clone()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<TaskData, TaskData>());
            return Mapper.Map<TaskData>(this);
        }
    }
}
