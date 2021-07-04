namespace LibiadaWeb.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;
    using System.Web;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;

    using Newtonsoft.Json;

    using SystemTask = System.Threading.Tasks.Task;

    /// <summary>
    /// The task manager.
    /// </summary>
    public class TaskManager
    {
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The instance.
        /// </summary>
        private static volatile TaskManager instance;

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        private readonly List<Task> tasks = new List<Task>();

        /// <summary>
        /// The signalr hub.
        /// </summary>
        private readonly TasksManagerHub signalrHub = new TasksManagerHub();

        /// <summary>
        /// Prevents a default instance of the <see cref="TaskManager"/> class from being created.
        /// </summary>
        private TaskManager()
        {
            RemoveGarbageFromDb();
            using (var db = new LibiadaWebEntities())
            {
                CalculationTask[] databaseTasks = db.CalculationTask.OrderBy(t => t.Created).ToArray();
                lock (tasks)
                {
                    foreach (CalculationTask task in databaseTasks)
                    {
                        tasks.Add(new Task(task));
                    }
                }
            }

        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static TaskManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new TaskManager();
                        }
                    }
                }

                return instance;
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

            using (var db = new LibiadaWebEntities())
            {
                databaseTask = new CalculationTask
                {
                    Created = DateTime.Now,
                    Description = taskType.GetDisplayValue(),
                    Status = TaskState.InQueue,
                    UserId = AccountHelper.GetUserId(),
                    TaskType = taskType
                };

                db.CalculationTask.Add(databaseTask);
                db.SaveChanges();
            }

            lock (tasks)
            {
                task = new Task(databaseTask.Id, action, databaseTask.UserId, taskType);
                lock (task)
                {
                    tasks.Add(task);
                    signalrHub.Send(TaskEvent.AddTask, task.TaskData);
                }
            }

            ManageTasks();
            return task.TaskData.Id;
        }

        /// <summary>
        /// Deletes all visible to user tasks.
        /// </summary>
        public void DeleteAllTasks()
        {
            List<Task> tasksToDelete = GetUserTasks();

            for (int i = tasksToDelete.Count - 1; i >= 0; i--)
            {
                DeleteTask(tasksToDelete[i].TaskData.Id);
            }
        }

        /// <summary>
        /// Deletes tasks with the specified state.
        /// </summary>
        /// <param name="taskState">
        /// The task state.
        /// </param>
        public void DeleteTasksWithState(TaskState taskState)
        {
            List<Task> tasksToDelete = GetUserTasksWithState(taskState);

            for (int i = tasksToDelete.Count - 1; i >= 0; i--)
            {
                DeleteTask(tasksToDelete[i].TaskData.Id);
            }
        }

        /// <summary>
        /// Deletes the task by id.
        /// </summary>
        /// <param name="id">
        /// The task id.
        /// </param>
        public void DeleteTask(long id)
        {
            lock (tasks)
            {
                Task task = tasks.Single(t => t.TaskData.Id == id);
                if (task.TaskData.UserId == AccountHelper.GetUserId() || AccountHelper.IsAdmin())
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

                        using (var db = new LibiadaWebEntities())
                        {
                            CalculationTask databaseTask = db.CalculationTask.Find(id);
                            db.CalculationTask.Remove(databaseTask);
                            db.SaveChanges();
                        }

                        signalrHub.Send(TaskEvent.DeleteTask, task.TaskData);
                    }
                }
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
                    if (!AccountHelper.IsAdmin() && task.TaskData.UserId != AccountHelper.GetUserId())
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
            var taskStatus = task.TaskData.TaskState;
            if (taskStatus != TaskState.Completed && taskStatus != TaskState.Error)
            {
                throw new Exception("Task state is not 'complete'");
            }

            using (var db = new LibiadaWebEntities())
            {
                return db.TaskResult.Single(tr => tr.TaskId == id && tr.Key == key).Value;
            }
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

                if (!AccountHelper.IsAdmin())
                {
                    var userId = AccountHelper.GetUserId();
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
            using (var db = new LibiadaWebEntities())
            {
                var tasksToDelete = db.CalculationTask
                    .Where(t => t.Status != TaskState.Completed && t.Status != TaskState.Error)
                    .ToArray();

                db.CalculationTask.RemoveRange(tasksToDelete);
                db.SaveChanges();
            }
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
                    foreach (Task taskToStart in tasksToStart)
                    {
                        lock (taskToStart)
                        {
                            Task task = tasks.Single(t => t.TaskData.Id == taskToStart.TaskData.Id);
                            task.TaskData.TaskState = TaskState.InProgress;
                            var cancellationTokenSource = new CancellationTokenSource();
                            CancellationToken token = cancellationTokenSource.Token;
                            task.CancellationTokenSource = cancellationTokenSource;
                            var systemTask = new SystemTask(() =>
                            {
                                using (cancellationTokenSource.Token.Register(Thread.CurrentThread.Abort))
                                {
                                    ExecuteTaskAction(task);
                                }

                            }, token);

                            SystemTask notificationTask = systemTask.ContinueWith((SystemTask t) =>
                            {
                                cancellationTokenSource.Dispose();

                                var data = new Dictionary<string, string>
                                {
                                    { "title", $"LibiadaWeb: Task completed" },
                                    { "body", $"Task type: { task.TaskData.TaskType.GetDisplayValue() } \nExecution time: { task.TaskData.ExecutionTime }" },
                                    { "icon", "/Content/DNA.png" },
                                    { "tag", $"/{ task.TaskData.TaskType }/Result/{ task.TaskData.Id }" }
                                };
                                PushNotificationHelper.Send(task.TaskData.UserId, data);
                            });

                            task.SystemTask = systemTask;
                            systemTask.Start();
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
                Func<Dictionary<string, string>> actionToCall;
                lock (task)
                {
                    actionToCall = task.Action;
                    task.TaskData.Started = DateTime.Now;
                    using (var db = new LibiadaWebEntities())
                    {
                        CalculationTask databaseTask = db.CalculationTask.Single(t => t.Id == task.TaskData.Id);

                        databaseTask.Started = DateTime.Now;
                        databaseTask.Status = TaskState.InProgress;
                        db.Entry(databaseTask).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    signalrHub.Send(TaskEvent.ChangeStatus, task.TaskData);
                }

                // executing action
                Dictionary<string, string> result = actionToCall();

                lock (task)
                {
                    task.TaskData.Completed = DateTime.Now;
                    task.TaskData.ExecutionTime = task.TaskData.Completed - task.TaskData.Started;
                    TaskResult[] results = result.Select(r => new TaskResult { Key = r.Key, Value = r.Value, TaskId = task.TaskData.Id }).ToArray();

                    task.TaskData.TaskState = TaskState.Completed;
                    using (var db = new LibiadaWebEntities())
                    {
                        db.TaskResult.AddRange(results);

                        CalculationTask databaseTask = db.CalculationTask.Single(t => (t.Id == task.TaskData.Id));
                        databaseTask.Completed = DateTime.Now;
                        databaseTask.Status = TaskState.Completed;

                        db.Entry(databaseTask).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    signalrHub.Send(TaskEvent.ChangeStatus, task.TaskData);
                }
            }
            catch (ThreadAbortException)
            {
                // TODO: implement an exception handling/logging
            }
            catch (Exception e)
            {
                string errorMessage = e.Message;
                string stackTrace = e.StackTrace;

                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    errorMessage += $"{Environment.NewLine} {e.Message}";
                }

                lock (task)
                {
                    var taskData = task.TaskData;
                    taskData.Completed = DateTime.Now;
                    taskData.ExecutionTime = taskData.Completed - taskData.Started;
                    taskData.TaskState = TaskState.Error;

                    var error = new { Message = errorMessage, StackTrace = stackTrace };

                    TaskResult taskResult = new TaskResult
                    {
                         Key = "Error", Value = JsonConvert.SerializeObject(error), TaskId = taskData.Id
                    };

                    using (var db = new LibiadaWebEntities())
                    {
                        db.TaskResult.Add(taskResult);

                        CalculationTask databaseTask = db.CalculationTask.Single(t => (t.Id == taskData.Id));
                        databaseTask.Completed = DateTime.Now;
                        databaseTask.Status = TaskState.Error;

                        db.Entry(databaseTask).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    signalrHub.Send(TaskEvent.ChangeStatus, taskData);
                }
            }
            finally
            {
                ManageTasks();
            }
        }
    }
}
