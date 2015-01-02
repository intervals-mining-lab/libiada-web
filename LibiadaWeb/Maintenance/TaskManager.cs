using System;
using System.Collections.Generic;
using System.Threading;

namespace LibiadaWeb.Maintenance
{
    public static class TaskManager
    {
        private static readonly Dictionary<int, Task> tasks = new Dictionary<int, Task>();
        private static int taskCounter = 0;
        private static int coreCount = Environment.ProcessorCount;

        public static int CreateNewTask(Task task)
        {
            tasks.Add(taskCounter, task);
            return taskCounter++;
        }

        public static void StartTask(int id)
        {
            var taskFromList = tasks[id];
            bool taskCheck;
            lock (taskFromList)
            {
                taskCheck = taskFromList != null && taskFromList.TaskData.TaskState == TaskState.InQueue;
            }
            if (taskCheck)
            {
                Action<Task> action = (task) =>
                {
                    try
                    {
                        Func<Dictionary<string, object>> method;
                        lock (task)
                        {
                            method = task.Action;
                            task.TaskData.TaskState = TaskState.InProgress;
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
                            task.TaskData.TaskState = TaskState.Error;
                            task.Result = new Dictionary<string, object>
                            {
                                { "Error", true }, 
                                { "ErrorMessage", e.Message },
                                { "StackTrace", e.StackTrace }
                            };
                        }
                    }
                };

                Thread thread = new Thread(() => action(taskFromList));
                thread.Start();
            }
        }

        public static Task GetTask(int id)
        {
            return tasks[id];
        }

        public static Dictionary<int, Task> Tasks
        {
            get { return tasks; }
        }

    }
}