namespace LibiadaWeb.Tasks
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Models;

    using Microsoft.AspNet.SignalR;

    using Newtonsoft.Json;

    /// <summary>
    /// SignalR messages hub class.
    /// </summary>
    public class TasksManagerHub : Hub
    {
        /// <summary>
        /// Send web socket message to all clients.
        /// </summary>
        /// <param name="taskEvent">
        /// Task event.
        /// </param>
        /// <param name="task">
        /// Task itself.
        /// </param>
        public static void Send(TaskEvent taskEvent, TaskData task)
        {
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<TasksManagerHub>();

            lock (task)
            {
                var result = new
                {
                    task.Id,
                    DisplayName = task.TaskState.GetDisplayValue(),
                    Created = task.Created.ToString(OutputFormats.DateTimeFormat),
                    Started = task.Started?.ToString(OutputFormats.DateTimeFormat),
                    Completed = task.Completed?.ToString(OutputFormats.DateTimeFormat),
                    ExecutionTime = task.ExecutionTime?.ToString(OutputFormats.TimeFormat),
                    TaskState = task.TaskState.ToString(),
                    TaskStateName = task.TaskState.GetDisplayValue(),
                    task.UserId,
                    task.UserName
                };

                hubContext.Clients.All.TaskEvent(taskEvent.ToString(), result);
            }
        }

        /// <summary>
        /// Called by clients on connect.
        /// </summary>
        /// <returns>
        /// The JSON of all tasks as <see cref="string"/>.
        /// </returns>
        public string GetAllTasks()
        {
            var tasks = TaskManager.GetTasksData().Select(t => new
            {
                t.Id,
                DisplayName = t.TaskType.GetDisplayValue(),
                Created = t.Created.ToString(OutputFormats.DateTimeFormat),
                Started = t.Started?.ToString(OutputFormats.DateTimeFormat),
                Completed = t.Completed?.ToString(OutputFormats.DateTimeFormat),
                ExecutionTime = t.ExecutionTime?.ToString(OutputFormats.TimeFormat),
                TaskState = t.TaskState.ToString(),
                TaskStateName = t.TaskState.GetDisplayValue(),
                t.UserId,
                t.UserName
            });

            return JsonConvert.SerializeObject(tasks.ToArray());

        }
    }
}