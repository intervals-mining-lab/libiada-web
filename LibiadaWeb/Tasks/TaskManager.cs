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
        /// Machine cores count.
        /// </summary>
        private readonly int coresCount = Environment.ProcessorCount;

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
                        if (task.Thread != null && task.Thread.IsAlive)
                        {
                            task.Thread.Abort();
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
        /// The manage tasks.
        /// </summary>
        private void ManageTasks()
        {
            lock (tasks)
            {
                int activeTasks = 0;
                foreach (Task task in tasks)
                {
                    lock (task)
                    {
                        if (task.TaskData.TaskState == TaskState.InProgress)
                        {
                            activeTasks++;
                        }
                    }
                }

                while (activeTasks < coresCount)
                {
                    activeTasks++;
                    Task taskToStart = tasks.FirstOrDefault(t => t.TaskData.TaskState == TaskState.InQueue);
                    if (taskToStart != null)
                    {
                        lock (taskToStart)
                        {
                            taskToStart.TaskData.TaskState = TaskState.InProgress;
                            StartTask(taskToStart.TaskData.Id);
                        }
                    }
                    else
                    {
                        break;
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

            ManageTasks();
        }

        /// <summary>
        /// The start task.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        private void StartTask(long id)
        {
            Task taskToStart;
            lock (tasks)
            {
                taskToStart = tasks.Single(t => t.TaskData.Id == id);
            }

            var thread = new Thread(() => ExecuteTaskAction(taskToStart));
            taskToStart.Thread = thread;
            thread.Start();
        }
    }
}
