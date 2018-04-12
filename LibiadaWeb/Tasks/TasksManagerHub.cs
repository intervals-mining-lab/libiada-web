namespace LibiadaWeb.Tasks
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
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
                if (!AccountHelper.IsAdmin())
                {
                    hubContext.Clients.Group(userId.ToString()).TaskEvent(taskEvent.ToString(), result);
                }

                hubContext.Clients.Group("admins").TaskEvent(taskEvent.ToString(), result);
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

            var tasks = TaskManager.Instance.GetTasksData()
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

        public override System.Threading.Tasks.Task OnConnected()
        {
            if (AccountHelper.IsAdmin())
            {
                Groups.Add(Context.ConnectionId, "admins");
            }
            else
            {
                Groups.Add(Context.ConnectionId, Context.User.Identity.GetUserId<int>().ToString());
            }
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            if (AccountHelper.IsAdmin())
            {
                Groups.Remove(Context.ConnectionId, "admins"); // сомневаюсь что это правильно
            }
            else
            {
                Groups.Remove(Context.ConnectionId, Context.User.Identity.GetUserId<int>().ToString());
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}
