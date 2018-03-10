using LibiadaWeb.Helpers;

namespace LibiadaWeb.Tasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.SignalR;

    using Newtonsoft.Json;

    /// <summary>
    /// SignalR messages hub class.
    /// </summary>
    [Authorize]
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
        public void Send(TaskEvent taskEvent, TaskData task)
        {
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<TasksManagerHub>();

            lock (task)
            {
                var result = new
                {
                    task.Id,
                    TaskType = task.TaskType.GetName(),
                    DisplayName = task.TaskType.GetDisplayValue(),
                    Created = task.Created.ToString(OutputFormats.DateTimeFormat),
                    Started = task.Started?.ToString(OutputFormats.DateTimeFormat),
                    Completed = task.Completed?.ToString(OutputFormats.DateTimeFormat),
                    ExecutionTime = task.ExecutionTime?.ToString(OutputFormats.TimeFormat),
                    TaskState = task.TaskState.ToString(),
                    TaskStateName = task.TaskState.GetDisplayValue(),
                    task.UserId,
                    task.UserName
                };

                int userId = task.UserId;
                List<string> usersId = new List<string>{
                    userId.ToString()
                };

                hubContext.Clients.Users(usersId).TaskEvent(taskEvent.ToString(), result);
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
            int userId = AccountHelper.GetUserId();
            bool isAdmin = AccountHelper.IsAdmin();

            var tasks = TaskManager.GetTasksData()
                .Where(t => t.UserId == userId || isAdmin)
                .Select(task => new
            {
                task.Id,
                TaskType = task.TaskType.GetName(),
                DisplayName = task.TaskType.GetDisplayValue(),
                Created = task.Created.ToString(OutputFormats.DateTimeFormat),
                Started = task.Started?.ToString(OutputFormats.DateTimeFormat),
                Completed = task.Completed?.ToString(OutputFormats.DateTimeFormat),
                ExecutionTime = task.ExecutionTime?.ToString(OutputFormats.TimeFormat),
                TaskState = task.TaskState.ToString(),
                TaskStateName = task.TaskState.GetDisplayValue(),
                task.UserId,
                task.UserName
            });

            return JsonConvert.SerializeObject(tasks.ToArray());
        }
    }
}
