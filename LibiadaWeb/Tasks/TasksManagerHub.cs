namespace LibiadaWeb.Tasks
{
    using LibiadaWeb.Models;
    using LibiadaWeb.Helpers;

    using Microsoft.AspNet.SignalR;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

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
        public void Send(TaskEvent taskEvent, TaskData task)
        {
            // Clients.All.broadcastMessage(name, message);
            lock (task)
            {
                var result = new
                {
                    task.Id,
                    task.DisplayName,
                    Created = task.Created.ToString(OutputFormats.DateTimeFormat),
                    Started = task.Started == null ? string.Empty : ((DateTimeOffset)task.Started).ToString(OutputFormats.DateTimeFormat),
                    Completed = task.Completed == null ? string.Empty : ((DateTimeOffset)task.Completed).ToString(OutputFormats.DateTimeFormat),
                    ExecutionTime = task.ExecutionTime == null ? string.Empty : ((TimeSpan)task.ExecutionTime).ToString(OutputFormats.TimeFormat),
                    TaskState = task.TaskState.ToString(),
                    TaskStateName = task.TaskState.GetDisplayValue(),
                    task.UserId,
                    task.UserName
                };
                var context = GlobalHost.ConnectionManager.GetHubContext<TasksManagerHub>();
                context.Clients.All.TaskEvent(taskEvent.ToString(), result);
            }
        }
    }
}
