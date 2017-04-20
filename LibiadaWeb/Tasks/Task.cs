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
        public readonly TaskType taskType;

        /// <summary>
        /// The task data.
        /// </summary>
        public readonly TaskData TaskData;

        /// <summary>
        /// The controller name.
        /// </summary>
        public readonly string ControllerName;

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
        /// <param name="controllerName">
        /// The controller name.
        /// </param>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        /// <param name="userId">
        /// Creator id.
        /// </param>
        public Task(int id, Func<Dictionary<string, object>> action, string controllerName, string displayName, string userId)
        {
            Action = action;
            ControllerName = controllerName;
            TaskData = new TaskData(id, displayName, userId);
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
        /// <param name="controllerName">
        /// The controller name.
        /// </param>
        /// <param name="thread">
        /// The thread.
        /// </param>
        private Task(Func<Dictionary<string, object>> action, TaskData taskData, Dictionary<string, object> result, string controllerName, Thread thread)
        {
            Action = action;
            TaskData = taskData.Clone();
            Result = result;
            ControllerName = controllerName;
            Thread = thread;
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task Clone()
        {
            return new Task(Action, TaskData, Result, ControllerName, Thread);
        }
    }
}
