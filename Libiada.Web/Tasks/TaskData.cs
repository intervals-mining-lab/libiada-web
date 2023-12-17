namespace Libiada.Web.Tasks
{
    using System;


    using Libiada.Web.Helpers;

    using Libiada.Database.Tasks;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// The task data.
    /// </summary>
    public class TaskData
    {
        /// <summary>
        /// The id.
        /// </summary>
        public readonly long Id;

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
        public TaskData(long id, IdentityUser<int> creator, TaskType taskType)
        {
            Id = id;
            TaskType = taskType;
            UserId = creator.Id;
            UserName = creator.UserName;
            Created = DateTime.Now;
            TaskState = TaskState.InQueue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskData"/> class.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        public TaskData(CalculationTask task)
        {
            Id = task.Id;
            TaskType = task.TaskType;
            UserId = task.UserId;
            UserName = task.AspNetUser.UserName;
            Created = task.Created;
            Started = task.Started;
            Completed = task.Completed;
            ExecutionTime = Completed - Started;
            TaskState = task.Status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskData"/> class.
        /// </summary>
        /// <param name="source">The other <see cref="TaskData"/> object to clone from</param>
        private TaskData(TaskData source)
        {
            Id = source.Id;
            TaskType = source.TaskType;
            UserId = source.UserId;
            UserName = source.UserName;
            Created = source.Created;
            Started = source.Started;
            Completed = source.Completed;
            ExecutionTime = source.ExecutionTime;
            TaskState = source.TaskState;
        }

        /// <summary>
        /// Creactes copy of current object.
        /// </summary>
        /// <returns>
        /// The <see cref="TaskData"/>.
        /// </returns>
        public TaskData Clone()
        {
            return new TaskData(this);
        }
    }
}
