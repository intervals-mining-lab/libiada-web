namespace LibiadaWeb.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;

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
                CalculationTask[] databaseTasks = db.CalculationTask.OrderByDescending(t => t.Created).ToArray();
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
        public long CreateTask(Func<Dictionary<string, object>> action, TaskType taskType)
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
                    UserId = Convert.ToInt32(AccountHelper.GetUserId()),
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
            List<Task> result = GetUserTasks();
            return result.Select(t => t.TaskData.Clone());
        }

        /// <summary>
        /// The get task.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task GetTask(int id)
        {
            Task result;
            lock (tasks)
            {
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
        /// Removes not finished amd not started tasks from db
        /// on task manager initialization.
        /// </summary>
        private void RemoveGarbageFromDb()
        {
            using (var db = new LibiadaWebEntities())
            {
                var tasksToDelete = db.CalculationTask
                    .Where(t => (t.Status != TaskState.Completed && t.Status != TaskState.Error) || t.Result == null)
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
                            CancellationTokenSource cts = new CancellationTokenSource();
                            CancellationToken token = cts.Token;
                            task.CancellationTokenSource = cts;
                            SystemTask systemTask = new SystemTask(() =>
                            {
                                using (cts.Token.Register(Thread.CurrentThread.Abort))
                                {
                                    ExecuteTaskAction(task);
                                }

                            }, token);
                                
                            SystemTask notificationTask = systemTask.ContinueWith((SystemTask t) =>
                            {
                                cts.Dispose();

                                var data = new Dictionary<string, string>
                                {
                                    { "title", $"Task has been completed" },
                                    { "body", $"Task type: { task.TaskData.TaskType } \nExecution time: { task.TaskData.ExecutionTime }" },
                                    { "icon", "/Content/themes/base/images/DNA.jpg" },
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
                Func<Dictionary<string, object>> actionToCall;
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
                Dictionary<string, object> result = actionToCall();

                lock (task)
                {
                    task.TaskData.Completed = DateTime.Now;
                    task.TaskData.ExecutionTime = task.TaskData.Completed - task.TaskData.Started;
                    task.Result = result;
                    task.TaskData.TaskState = TaskState.Completed;
                    using (var db = new LibiadaWebEntities())
                    {
                        CalculationTask databaseTask = db.CalculationTask.Single(t => (t.Id == task.TaskData.Id));

                        databaseTask.Completed = DateTime.Now;
                        databaseTask.Status = TaskState.Completed;
                        if (result.ContainsKey("data"))
                        {
                            databaseTask.Result = result["data"].ToString();
                        }

                        if (result.ContainsKey("additionalData"))
                        {
                            databaseTask.AdditionalResultData = JsonConvert.SerializeObject(result["additionalData"]);
                        }

                        db.Entry(databaseTask).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    signalrHub.Send(TaskEvent.ChangeStatus, task.TaskData);
                }
            }
            catch (ThreadAbortException e)
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
                    task.TaskData.Completed = DateTime.Now;
                    task.TaskData.ExecutionTime = task.TaskData.Completed - task.TaskData.Started;
                    task.TaskData.TaskState = TaskState.Error;
                    task.Result = new Dictionary<string, object>
                                      {
                                          { "Error", true },
                                          { "ErrorMessage", errorMessage },
                                          { "StackTrace", stackTrace }
                                      };

                    using (var db = new LibiadaWebEntities())
                    {
                        CalculationTask databaseTask = db.CalculationTask.Single(t => (t.Id == task.TaskData.Id));

                        databaseTask.Completed = DateTime.Now;
                        databaseTask.Status = TaskState.Error;
                        databaseTask.Result = JsonConvert.SerializeObject(task.Result);
                        db.Entry(databaseTask).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    signalrHub.Send(TaskEvent.ChangeStatus, task.TaskData);
                }
            }
            finally
            {
                ManageTasks();
            }
        }
    }
}
