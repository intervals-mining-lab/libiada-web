namespace LibiadaWeb.Tasks
{
    using Libiada.Database.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using SystemTask = System.Threading.Tasks.Task;

    /// <summary>
    /// The task.
    /// </summary>
    public class Task
    {
        /// <summary>
        /// The action.
        /// </summary>
        public readonly Func<Dictionary<string, string>> Action;

        /// <summary>
        /// The task data.
        /// </summary>
        public readonly TaskData TaskData;

        /// <summary>
        /// The result.
        /// </summary>
        //public Dictionary<string, string> Result;

        /// <summary>
        /// The cancellation token source to delete the task.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource;

        /// <summary>
        /// The system task that executes asynchronously on a thread pool.
        /// </summary>
        public SystemTask SystemTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="userId">
        /// Creator id.
        /// </param>
        /// <param name="taskType">
        /// The task Type.
        /// </param>
        public Task(long id, Func<Dictionary<string, string>> action, int userId, TaskType taskType)
        {
            Action = action;
            TaskData = new TaskData(id, userId, taskType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        public Task(CalculationTask task)
        {
            TaskData = new TaskData(task);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="taskData">
        /// The task data.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="thread">
        /// The thread.
        /// </param>
        private Task(Func<Dictionary<string, string>> action, TaskData taskData, SystemTask systemTask)
        {
            Action = action;
            TaskData = taskData.Clone();
            SystemTask = systemTask;
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task Clone()
        {
            return new Task(Action, TaskData, SystemTask);
        }
    }
}
