﻿namespace Libiada.Web.Tasks
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Libiada.Database.Tasks;
    using LibiadaCore.Extensions;

    using Libiada.Web.Helpers;

    using Newtonsoft.Json;

    using SystemTask = System.Threading.Tasks.Task;
    using Libiada.Web.Extensions;

    /// <summary>
    /// The task manager.
    /// </summary>
    public class TaskManager : ITaskManager
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        private readonly List<Task> tasks = new List<Task>();

        /// <summary>
        /// The signalr hub.
        /// </summary>
        private readonly TaskManagerHub signalrHub;

        public TaskManager(ILibiadaDatabaseEntitiesFactory dbFactory, ITaskManagerHubFactory factory, IHttpContextAccessor httpContextAccessor)
        {

            db = dbFactory.CreateDbContext();
            this.httpContextAccessor = httpContextAccessor;
            signalrHub = factory.Create(this);
            RemoveGarbageFromDb();
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
            IPrincipal user = httpContextAccessor.HttpContext.User;
            databaseTask = new CalculationTask
            {
                Created = DateTime.Now,
                Description = taskType.GetDisplayValue(),
                Status = TaskState.InQueue,
                UserId = user.GetUserId(),
                TaskType = taskType
            };

            db.CalculationTasks.Add(databaseTask);
            db.SaveChanges();

            lock (tasks)
            {
                task = new Task(databaseTask.Id, action, databaseTask.AspNetUser, taskType);
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
                IPrincipal user = httpContextAccessor.HttpContext.User;
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

                        CalculationTask databaseTask = db.CalculationTasks.Find(id);
                        db.CalculationTasks.Remove(databaseTask);
                        db.SaveChanges();

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
                    IPrincipal user = httpContextAccessor.HttpContext.User;
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
            var taskStatus = task.TaskData.TaskState;
            if (taskStatus != TaskState.Completed && taskStatus != TaskState.Error)
            {
                throw new Exception("Task state is not 'complete'");
            }

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
                IPrincipal user = httpContextAccessor.HttpContext.User;
                if (!user.IsAdmin())
                {
                    var userId = user.GetUserId();
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
                                    ExecuteTaskAction((Task)t);
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
                                    PushNotificationHelper.Send(db, task.TaskData.UserId, data);
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
                Func<Dictionary<string, string>> actionToCall;
                lock (task)
                {
                    actionToCall = task.Action;
                    task.TaskData.Started = DateTime.Now;
                    CalculationTask databaseTask = db.CalculationTasks.Single(t => t.Id == task.TaskData.Id);

                    databaseTask.Started = DateTime.Now;
                    databaseTask.Status = TaskState.InProgress;
                    db.Entry(databaseTask).State = EntityState.Modified;
                    db.SaveChanges();


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
                    db.TaskResults.AddRange(results);

                    CalculationTask databaseTask = db.CalculationTasks.Single(t => (t.Id == task.TaskData.Id));
                    databaseTask.Completed = task.TaskData.Completed?.DateTime;
                    databaseTask.Status = TaskState.Completed;
                    db.Entry(databaseTask).State = EntityState.Modified;

                    db.SaveChanges();

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
                        Key = "Error",
                        Value = JsonConvert.SerializeObject(error),
                        TaskId = taskData.Id
                    };

                    db.TaskResults.Add(taskResult);

                    CalculationTask databaseTask = db.CalculationTasks.Single(t => (t.Id == taskData.Id));
                    databaseTask.Completed = DateTime.Now;
                    databaseTask.Status = TaskState.Error;
                    db.Entry(databaseTask).State = EntityState.Modified;

                    db.SaveChanges();

                    signalrHub.Send(TaskEvent.ChangeStatus, taskData);
                }
            }
        }
    }
}