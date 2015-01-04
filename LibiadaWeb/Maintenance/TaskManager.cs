using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LibiadaWeb.Maintenance
{
    public static class TaskManager
    {
        private static readonly List<Task> tasks = new List<Task>();
        private static int taskCounter = 0;
        private static int coreCount = Environment.ProcessorCount;

        public static void AddTask(Task task)
        {
            tasks.Add(task);
            ManageTasks();
        }

        public static int GetId()
        {
            return taskCounter++;
        }

        public static void ClearTasks()
        {
            lock (tasks)
            {
                while (tasks.Count > 0)
                {
                    var task = tasks.Last();
                    lock (task)
                    {
                        if (task.Thread != null && task.Thread.IsAlive)
                        {
                            task.Thread.Abort();
                        }

                        tasks.Remove(task);
                    }
                }
            }
        }

        public static void DeleteTask(int id)
        {
            var task = tasks.Single(t => t.TaskData.Id == id);
            lock (task)
            {
                if (task.Thread != null && task.Thread.IsAlive)
                {
                    task.Thread.Abort();
                }

                tasks.Remove(task);
            }
        }

        private static void StartTask(int id)
        {
            var taskToStart = tasks.Single(t => t.TaskData.Id == id);
            Action<Task> action = (task) =>
            {
                try
                {
                    Func<Dictionary<string, object>> method;
                    lock (task)
                    {
                        method = task.Action;
                    }

                    var result = method();
                    lock (task)
                    {
                        task.Result = result;
                        task.TaskData.TaskState = TaskState.Completed;
                    }
                }
                catch (Exception e)
                {
                    lock (task)
                    {
                        string errorMessage = e.Message;
                        string stackTrace = e.StackTrace;

                        while (e.InnerException != null)
                        {
                            e = e.InnerException;
                            errorMessage += "<br/>" + e.Message;
                        }

                        task.Result = new Dictionary<string, object>
                            {
                                { "Error", true }, 
                                { "ErrorMessage", errorMessage },
                                { "StackTrace", stackTrace }
                            };

                        task.TaskData.TaskState = TaskState.Error;
                    }
                }

                ManageTasks();
            };

            var thread = new Thread(() => action(taskToStart));
            taskToStart.Thread = thread;
            thread.Start();
        }

        public static Task GetTask(int id)
        {
            return tasks.Single(t => t.TaskData.Id == id);
        }

        public static List<Task> Tasks
        {
            get { return tasks; }
        }

        public static void ManageTasks()
        {
            lock (Tasks)
            {
                int activeTasks = 0;
                foreach (var task in Tasks)
                {
                    lock (task)
                    {
                        if (task.TaskData.TaskState == TaskState.InProgress)
                        {
                            activeTasks++;
                        }
                    }
                }
                while (activeTasks < coreCount)
                {
                    activeTasks++;
                    var taskToStart = Tasks.FirstOrDefault(t => t.TaskData.TaskState == TaskState.InQueue);
                    if (taskToStart != null)
                    {
                        lock (taskToStart)
                        {
                            if (taskToStart != null)
                            {
                                taskToStart.TaskData.TaskState = TaskState.InProgress;
                                StartTask(taskToStart.TaskData.Id);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

    }
}