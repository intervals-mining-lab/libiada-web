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
    public static class TaskManager
    {
        /// <summary>
        /// Machine cores count.
        /// </summary>
        private static readonly int CoresCount = Environment.ProcessorCount;

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        private static readonly List<Task> Tasks = new List<Task>();

        /// <summary>
        /// The created tasks counter.
        /// </summary>
        private static int taskCounter;

        /// <summary>
        /// Adds new task to db and tasks list.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="taskType">
        /// The task Type.
        /// </param>
        public static void CreateTask(Func<Dictionary<string, object>> action, TaskType taskType)
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

            lock (Tasks)
            {
                task = new Task(databaseTask.Id, action, databaseTask.UserId, taskType);
                Tasks.Add(task);
            }

            TasksManagerHub.Send(TaskEvent.AddTask, task.TaskData);
            ManageTasks();
        }

        /// <summary>
        /// The get id.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetId()
        {
            return taskCounter++;
        }

        /// <summary>
        /// The clear tasks.
        /// </summary>
        public static void DeleteAllTasks()
        {
            lock (Tasks)
            {
                List<Task> tasks = Tasks;

                if (!AccountHelper.IsAdmin())
                {
                    tasks = tasks.Where(t => t.TaskData.UserId == AccountHelper.GetUserId()).ToList();
                }

                while (tasks.Count > 0)
                {
                    Task task = tasks.Last();
                    lock (task)
                    {
                        if (task.Thread != null && task.Thread.IsAlive)
                        {
                            task.Thread.Abort();
                        }

                        tasks.Remove(task);
                        Tasks.Remove(task);

                        TasksManagerHub.Send(TaskEvent.DeleteTask, task.TaskData);
                    }
                }
            }
        }

        /// <summary>
        /// The delete task.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public static void DeleteTask(int id)
        {
            lock (Tasks)
            {
                Task task = Tasks.Single(t => t.TaskData.Id == id);
                if (task.TaskData.UserId == AccountHelper.GetUserId() || AccountHelper.IsAdmin())
                {
                    lock (task)
                    {
                        if (task.Thread != null && task.Thread.IsAlive)
                        {
                            task.Thread.Abort();
                        }

                        Tasks.Remove(task);
                        TasksManagerHub.Send(TaskEvent.DeleteTask, task.TaskData);
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
        public static IEnumerable<TaskData> GetTasksData()
        {
            lock (Tasks)
            {
                List<Task> tasks = Tasks;

                if (!AccountHelper.IsAdmin())
                {
                    tasks = tasks.Where(t => t.TaskData.UserId == AccountHelper.GetUserId()).ToList();
                }

                return tasks.Select(t => t.TaskData.Clone());
            }
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
        public static Task GetTask(int id)
        {
            Task result;
            lock (Tasks)
            {
                Task task = Tasks.Single(t => t.TaskData.Id == id);

                lock (task)
                {
                    if (!AccountHelper.IsAdmin() && task.TaskData.UserId != AccountHelper.GetUserId())
                    {
                        throw new AccessViolationException("You do not have access to current task");
                    }

                    result = task.Clone();
                }
            }

            return result;
        }

        /// <summary>
        /// The manage tasks.
        /// </summary>
        private static void ManageTasks()
        {
            lock (Tasks)
            {
                int activeTasks = 0;
                foreach (Task task in Tasks)
                {
                    lock (task)
                    {
                        if (task.TaskData.TaskState == TaskState.InProgress)
                        {
                            activeTasks++;
                        }
                    }
                }

                while (activeTasks < CoresCount)
                {
                    activeTasks++;
                    Task taskToStart = Tasks.FirstOrDefault(t => t.TaskData.TaskState == TaskState.InQueue);
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
        /// The create action.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        private static void ExecuteTaskAction(Task task)
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
                        CalculationTask dbTask = db.CalculationTask.Single(t => t.Id == task.TaskData.Id);

                        dbTask.Started = DateTime.Now;
                        dbTask.Status = TaskState.InProgress;
                        db.Entry(dbTask).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    TasksManagerHub.Send(TaskEvent.ChangeStatus, task.TaskData);
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
                        if (result.ContainsKey("data"))
                        {
                            databaseTask.Result = JsonConvert.SerializeObject(result["data"]);
                        }

                        if (result.ContainsKey("additionalData"))
                        {
                            databaseTask.AdditionalResultData = JsonConvert.SerializeObject(result["additionalData"]);
                        }

                        db.Entry(databaseTask).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    TasksManagerHub.Send(TaskEvent.ChangeStatus, task.TaskData);
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message;
                string stackTrace = e.StackTrace;

                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    errorMessage += "<br/>" + e.Message;
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
                    TasksManagerHub.Send(TaskEvent.ChangeStatus, task.TaskData);
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
        private static void StartTask(long id)
        {
            Task taskToStart;
            lock (Tasks)
            {
                taskToStart = Tasks.Single(t => t.TaskData.Id == id);
            }

            var thread = new Thread(() => ExecuteTaskAction(taskToStart));
            taskToStart.Thread = thread;
            thread.Start();
        }
    }
}
