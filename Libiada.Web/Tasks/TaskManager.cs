namespace Libiada.Web.Tasks;

using Microsoft.AspNetCore.SignalR;

using System.Security.Claims;
using System.Runtime.CompilerServices;

using Libiada.Database.Tasks;

using Libiada.Core.Extensions;

using Libiada.Web.Helpers;
using Libiada.Web.Extensions;

using Newtonsoft.Json;

using SystemTask = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// The task manager.
/// </summary>
public class TaskManager : ITaskManager
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Gets the tasks.
    /// </summary>
    private readonly List<Task> tasks = [];
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;

    /// <summary>
    /// The signalr hub.
    /// </summary>
    private readonly IHubContext<TaskManagerHub> signalrHubContext;
    private readonly IPushNotificationHelper pushNotificationHelper;

    public TaskManager(
        IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
        IHubContext<TaskManagerHub> signalrHubContext,
        IPushNotificationHelper pushNotificationHelper,
        IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.dbFactory = dbFactory;
        this.signalrHubContext = signalrHubContext;
        this.pushNotificationHelper = pushNotificationHelper;
        RemoveGarbageFromDb();
        using var db = dbFactory.CreateDbContext();
        CalculationTask[] databaseTasks = db.CalculationTasks.OrderBy(t => t.Created).Include(t => t.AspNetUser).ToArray();
        lock (tasks)
        {
            foreach (CalculationTask task in databaseTasks)
            {
                tasks.Add(new Task(task));
            }
        }
    }

    /// <summary>
    /// Adds new task to db and tasks list.
    /// </summary>
    /// <param name="action">
    /// The action.
    /// </param>
    /// <param name="taskType">
    /// The task Type.
    /// </param>
    public long CreateTask(Func<Dictionary<string, string>> action, TaskType taskType)
    {
        CalculationTask databaseTask;
        Task task;
        ClaimsPrincipal user = httpContextAccessor.GetCurrentUser();
        databaseTask = new CalculationTask
        {
            Description = taskType.GetDisplayValue(),
            Status = TaskState.InQueue,
            UserId = user.GetUserId(),
            TaskType = taskType
        };

        using var db = dbFactory.CreateDbContext();
        db.CalculationTasks.Add(databaseTask);
        db.SaveChanges();
        TaskAwaiter taskAwaiter;
        lock (tasks)
        {
            task = new Task(databaseTask.Id, action, new AspNetUser { Id = user.GetUserId(), UserName = user.Identity?.Name }, taskType);
            lock (task)
            {
                tasks.Add(task);
                taskAwaiter = SendTaskEventToClients(TaskEvent.AddTask, task.TaskData).GetAwaiter();
            }
        }
        
        ManageTasks();
        taskAwaiter.OnCompleted(() => { });
        return task.TaskData.Id;
    }

    /// <summary>
    /// Deletes all visible to user tasks.
    /// </summary>
    public IEnumerable<TaskData> DeleteAllTasks()
    {
        List<Task> tasksToDelete = GetUserTasks();
        List<TaskData> result = new(tasksToDelete.Count);
        for (int i = tasksToDelete.Count - 1; i >= 0; i--)
        {
            result.Add(DeleteTask(tasksToDelete[i].TaskData.Id));
        }

        return result;
    }

    /// <summary>
    /// Deletes tasks with the specified state.
    /// </summary>
    /// <param name="taskState">
    /// The task state.
    /// </param>
    public IEnumerable<TaskData> DeleteTasksWithState(TaskState taskState)
    {
        List<Task> tasksToDelete = GetUserTasksWithState(taskState);

        List<TaskData> result = new(tasksToDelete.Count);

        for (int i = tasksToDelete.Count - 1; i >= 0; i--)
        {
            result.Add(DeleteTask(tasksToDelete[i].TaskData.Id));
        }

        return result;
    }

    /// <summary>
    /// Deletes the task by id.
    /// </summary>
    /// <param name="id">
    /// The task id.
    /// </param>
    public TaskData? DeleteTask(long id)
    {
        lock (tasks)
        {
            ClaimsPrincipal user = httpContextAccessor.GetCurrentUser();
            Task task = tasks.Single(t => t.TaskData.Id == id);
            if (task.TaskData.UserId == user.GetUserId() || user.IsAdmin())
            {
                lock (task)
                {
                    if ((task.SystemTask != null) && (!task.SystemTask.IsCompleted))
                    {
                        CancellationTokenSource cancellationTokenSource = task.CancellationTokenSource;
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource.Dispose();
                    }

                    tasks.Remove(task);
                    using var db = dbFactory.CreateDbContext();
                    CalculationTask? databaseTask = db.CalculationTasks.Find(id);
                    if(databaseTask is not null)
                    {
                        db.CalculationTasks.Remove(databaseTask);
                        db.SaveChanges();
                    }
                    
                    return task.TaskData;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// The get tasks data.
    /// </summary>
    /// <returns>
    /// The <see cref="IEnumerable{TaskData}"/>.
    /// </returns>
    public IEnumerable<TaskData> GetTasksData()
    {
        return GetUserTasks().Select(t => t.TaskData.Clone());
    }

    /// <summary>
    /// The get task.
    /// </summary>
    /// <param name="id">
    /// The task id in database.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    public Task GetTask(long id)
    {
        Task result;
        lock (tasks)
        {
            // TODO: check if task with id exists
            Task task = tasks.Single(t => t.TaskData.Id == id);

            lock (task)
            {
                ClaimsPrincipal user = httpContextAccessor.GetCurrentUser();
                if (!user.IsAdmin() && task.TaskData.UserId != user.GetUserId())
                {
                    throw new AccessViolationException("You do not have access to the current task");
                }

                result = task.Clone();
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the task data by id.
    /// </summary>
    /// <param name="id">
    /// The task id in database.
    /// </param>
    /// <returns>
    /// The json as <see cref="string"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if task is not complete.
    /// </exception>
    public string GetTaskData(long id, string key = "data")
    {
        Task task = GetTask(id);
        TaskState taskStatus = task.TaskData.TaskState;
        if (taskStatus != TaskState.Completed && taskStatus != TaskState.Error)
        {
            throw new Exception("Task state is not 'complete'");
        }
        using var db = dbFactory.CreateDbContext();
        return db.TaskResults.Single(tr => tr.TaskId == id && tr.Key == key).Value;
    }

    /// <summary>
    /// Gets tasks available to user.
    /// </summary>
    /// <returns>
    /// The <see cref="T:List{Task}"/>.
    /// </returns>
    private List<Task> GetUserTasks()
    {
        lock (tasks)
        {
            List<Task> result = tasks;
            ClaimsPrincipal user = httpContextAccessor.GetCurrentUser();
            if (!user.IsAdmin())
            {
                int userId = user.GetUserId();
                result = result.Where(t => t.TaskData.UserId == userId).ToList();
            }

            return result;
        }
    }

    /// <summary>
    /// Gets tasks available to user with the specified state.
    /// </summary>
    /// <returns>
    /// The <see cref="T:List{Task}"/>.
    /// </returns>
    private List<Task> GetUserTasksWithState(TaskState taskState)
    {
        List<Task> result = GetUserTasks();
        lock (tasks)
        {
            result = result.Where(t => t.TaskData.TaskState == taskState).ToList();
            return result;
        }
    }

    /// <summary>
    /// Removes not finished and not started tasks from database
    /// on task manager initialization.
    /// </summary>
    private void RemoveGarbageFromDb()
    {
        using var db = dbFactory.CreateDbContext();
        var tasksToDelete = db.CalculationTasks
            .Where(t => t.Status != TaskState.Completed && t.Status != TaskState.Error)
            .ToArray();

        db.CalculationTasks.RemoveRange(tasksToDelete);
        db.SaveChanges();
    }

    /// <summary>
    /// Initializes new tasks and runs tasks scheduling it
    /// for execution to the current TaskScheduler.
    /// </summary>
    private void ManageTasks()
    {
        lock (tasks)
        {
            List<Task> tasksToStart = tasks.FindAll(t => t.TaskData.TaskState == TaskState.InQueue);
            if (tasksToStart.Count != 0)
            {
                foreach (Task task in tasksToStart)
                {
                    lock (task)
                    {
                        task.TaskData.TaskState = TaskState.InProgress;
                        var cancellationTokenSource = new CancellationTokenSource();
                        CancellationToken token = cancellationTokenSource.Token;
                        task.CancellationTokenSource = cancellationTokenSource;
                        var systemTask = SystemTask.Factory.StartNew((t) =>
                        {
                            using (cancellationTokenSource.Token.Register(Thread.CurrentThread.Abort))
                            {
                                ExecuteTaskAction((Task)t!);
                            }

                        }, task, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                        systemTask.ContinueWith(t =>
                        {
                            cancellationTokenSource.Dispose();
                            lock (task)
                            {
                                var data = new Dictionary<string, string>
                                {
                                    { "title", $"Libiada.Web: Task completed" },
                                    { "body", $"Task type: { task.TaskData.TaskType.GetDisplayValue() } \nExecution time: { task.TaskData.ExecutionTime }" },
                                    { "icon", "/Content/DNA.png" },
                                    { "tag", $"/{ task.TaskData.TaskType }/Result/{ task.TaskData.Id }" }
                                };
                                pushNotificationHelper.Send(task.TaskData.UserId, data);
                            }
                        });

                        task.SystemTask = systemTask;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Starts a new thread with the given task.
    /// </summary>
    /// <param name="task">
    /// The task.
    /// </param>
    private void ExecuteTaskAction(Task task)
    {
        try
        {
            TaskAwaiter taskAwaiter;
            Func<Dictionary<string, string>> actionToCall;
            lock (task)
            {
                actionToCall = task.Action;
                task.TaskData.Started = DateTimeOffset.UtcNow;
                using var db = dbFactory.CreateDbContext();
                CalculationTask databaseTask = db.CalculationTasks.Single(t => t.Id == task.TaskData.Id);

                databaseTask.Started = DateTimeOffset.UtcNow;
                databaseTask.Status = TaskState.InProgress;

                db.Entry(databaseTask).State = EntityState.Modified;
                db.SaveChanges();

                taskAwaiter = SendTaskEventToClients(TaskEvent.ChangeStatus, task.TaskData).GetAwaiter();
            }

            taskAwaiter.OnCompleted(() => { });
            // executing action
            Dictionary<string, string> result = actionToCall();

            lock (task)
            {
                task.TaskData.Completed = DateTimeOffset.UtcNow;
                task.TaskData.ExecutionTime = task.TaskData.Completed - task.TaskData.Started;
                TaskResult[] results = result.Select(r => new TaskResult { Key = r.Key, Value = r.Value, TaskId = task.TaskData.Id }).ToArray();

                task.TaskData.TaskState = TaskState.Completed;

                using var db = dbFactory.CreateDbContext();
                db.TaskResults.AddRange(results);

                CalculationTask databaseTask = db.CalculationTasks.Single(t => (t.Id == task.TaskData.Id));
                databaseTask.Completed = task.TaskData.Completed;
                databaseTask.Status = TaskState.Completed;
                db.Entry(databaseTask).State = EntityState.Modified;

                db.SaveChanges();
                taskAwaiter = SendTaskEventToClients(TaskEvent.ChangeStatus, task.TaskData).GetAwaiter();
            }

            taskAwaiter.OnCompleted(() => { });
        }
        catch (ThreadAbortException)
        {
            // TODO: implement an exception handling/logging
        }
        catch (Exception e)
        {
            string errorMessage = e.Message;
            string stackTrace = e.StackTrace ?? string.Empty;

            while (e.InnerException != null)
            {
                e = e.InnerException;
                errorMessage += $"{Environment.NewLine} {e.Message}";
            }
            TaskAwaiter taskAwaiter;
            lock (task)
            {
                TaskData taskData = task.TaskData;
                taskData.Completed = DateTimeOffset.UtcNow;
                taskData.ExecutionTime = taskData.Completed - taskData.Started;
                taskData.TaskState = TaskState.Error;

                var error = new { Message = errorMessage, StackTrace = stackTrace };

                TaskResult taskResult = new TaskResult
                {
                    Key = "Error",
                    Value = JsonConvert.SerializeObject(error),
                    TaskId = taskData.Id
                };
                using var db = dbFactory.CreateDbContext();
                db.TaskResults.Add(taskResult);

                CalculationTask databaseTask = db.CalculationTasks.Single(t => (t.Id == taskData.Id));
                databaseTask.Completed = DateTimeOffset.UtcNow;
                databaseTask.Status = TaskState.Error;
                db.Entry(databaseTask).State = EntityState.Modified;

                db.SaveChanges();
                taskAwaiter = SendTaskEventToClients(TaskEvent.ChangeStatus, taskData).GetAwaiter();
            }

            taskAwaiter.OnCompleted(() => { });
        }
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
    public async SystemTask SendTaskEventToClients(TaskEvent taskEvent, TaskData task)
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

        await signalrHubContext.Clients.Group("admins").SendAsync("TaskEvent", taskEvent.ToString(), result);
        using var db = dbFactory.CreateDbContext();
        var signInManager = (httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null"))
                            .RequestServices.GetService<SignInManager<AspNetUser>>();
        var claimsFactory = (signInManager ?? throw new Exception("SignInManager is null")).ClaimsFactory;
        ClaimsPrincipal user = await claimsFactory.CreateAsync(db.Users.Single(u => u.Id == task.UserId));
        if (!user.IsAdmin())
        {
            await signalrHubContext.Clients.Group(task.UserId.ToString()).SendAsync("TaskEvent", taskEvent.ToString(), result);
        }
    }
}
