namespace Libiada.Web.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Libiada.Database.Tasks;

using Libiada.Core.Extensions;

using Libiada.Web.Helpers;
using Libiada.Web.Extensions;

using SystemTask = System.Threading.Tasks.Task;

/// <summary>
/// SignalR messages hub class.
/// </summary>
[Authorize]
public class TaskManagerHub : Hub<ITaskManagerClient>
{
    private readonly ITaskManager taskManager;

    public TaskManagerHub(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    /// <summary>
    /// Send web socket message to target clients.
    /// </summary>
    /// <param name="taskEvent">
    /// Task event.
    /// </param>
    /// <param name="task">
    /// Task itself.
    /// </param>
    public async SystemTask Send(TaskEvent taskEvent, TaskData task)
    {
        object result;

        lock (task)
        {
            result = new
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
        }

        await Clients.Group("admins").TaskEvent(taskEvent.ToString(), result);
        if (!Context.User.IsAdmin())
        {
            await Clients.Group(task.UserId.ToString()).TaskEvent(taskEvent.ToString(), result);
        }
    }

    /// <summary>
    /// Called by clients on connect.
    /// </summary>
    /// <returns>
    /// The JSON of all tasks as <see cref="string"/>.
    /// </returns>
    public object[] GetAllTasks()
    {
        int userId = Context.User.GetUserId();
        bool isAdmin = Context.User.IsAdmin();

        var tasks = taskManager.GetTasksData()
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

        return tasks.ToArray();
    }

    public override async SystemTask OnConnectedAsync()
    {
        if (Context.User.IsAdmin())
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }
        else
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.GetUserId().ToString());
        }

        await base.OnConnectedAsync();
    }

    public override async SystemTask OnDisconnectedAsync(Exception? ex)
    {
        if (Context.User.IsAdmin())
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
        }
        else
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.GetUserId().ToString());
        }

        await base.OnDisconnectedAsync(ex);
    }

    /// <summary>
    /// Delete task by id.
    /// </summary>
    /// <param name="id">
    /// The id.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public async SystemTask DeleteTask(int id)
    {
        try
        {
            TaskData taskData = taskManager.DeleteTask(id);
            await Send(TaskEvent.DeleteTask, taskData);
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to delete task with id = {id}", e);
        }
    }

    /// <summary>
    /// Delete tasks with the specified state.
    /// </summary>
    /// <param name="taskState">
    /// The id.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public async SystemTask DeleteTasksWithState(TaskState taskState)
    {
        try
        {
            var tasksData = taskManager.DeleteTasksWithState(taskState);
            await SystemTask.WhenAll(tasksData.Select(td => Send(TaskEvent.DeleteTask, td)));
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to delete tasks with state '{taskState}'", e);
        }
    }

    /// <summary>
    /// Delete all tasks.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public async SystemTask DeleteAllTasks()
    {
        try
        {
            var tasksData = taskManager.DeleteAllTasks();
            await SystemTask.WhenAll(tasksData.Select(td => Send(TaskEvent.DeleteTask, td)));
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to delete all tasks", e);
        }
    }
}
