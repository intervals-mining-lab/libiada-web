namespace LibiadaWeb.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// The task.
    /// </summary>
    public class Task
    {
        /// <summary>
        /// The action.
        /// </summary>
        public readonly Func<Dictionary<string, object>> Action;

        /// <summary>
        /// The task type.
        /// </summary>
        public readonly TaskType TaskType;

        /// <summary>
        /// The task data.
        /// </summary>
        public readonly TaskData TaskData;

        /// <summary>
        /// The result.
        /// </summary>
        public Dictionary<string, object> Result;

        /// <summary>
        /// The thread.
        /// </summary>
        public Thread Thread;

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
        public Task(int id, Func<Dictionary<string, object>> action, string userId, TaskType taskType)
        {
            TaskType = taskType;
            Action = action;
            TaskData = new TaskData(id, userId);
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
        /// <param name="taskType">
        /// The task Type.
        /// </param>
        private Task(Func<Dictionary<string, object>> action, TaskData taskData, Dictionary<string, object> result, Thread thread, TaskType taskType)
        {
            Action = action;
            TaskData = taskData.Clone();
            Result = result;
            Thread = thread;
            TaskType = taskType;
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task Clone()
        {
            return new Task(Action, TaskData, Result, Thread, TaskType);
        }
    }
}
