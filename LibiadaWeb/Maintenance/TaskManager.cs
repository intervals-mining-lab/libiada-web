using System;
using System.Collections.Generic;
using System.Threading;

namespace LibiadaWeb.Maintenance
{
    public static class TaskManager
    {
        private static Dictionary<int, TaskData> tasks = new Dictionary<int, TaskData>();
        private static int taskCounter = 0;
        //private static Task task;
        private static int coreCount = Environment.ProcessorCount;

        public static int CreateNewTask(TaskData taskData)
        {
            tasks.Add(taskCounter, taskData);
            return taskCounter++;
        }

        public static void StartTask(int id)
        {
            var task = tasks[id];
            if (task != null && task.TaskState == TaskState.InQueue)
            {
                Action<TaskData> action = (taskData) =>
                {
                    taskData.TaskState = TaskState.InProcess;
                    try
                    {
                        taskData.Action();
                        taskData.TaskState = TaskState.Completed;
                    }
                    catch (Exception e)
                    {
                        taskData.TaskState = TaskState.Error;
                        taskData.Result = new Dictionary<string, object>
                        {
                            { "ErrorMessage", e.Message }, 
                            { "StackTrace", e.StackTrace }
                        };
                    }
                };
                Thread thread = new Thread(() => action(task));
                thread.Start();
            }
        }

        //private static void threadDo(TaskData taskData)
        //{
        //    taskData.TaskState = TaskState.InProcess;
        //    try
        //    {
        //        taskData.Action();
        //        taskData.TaskState = TaskState.Completed;
        //    }
        //    catch (Exception e)
        //    {
        //        taskData.TaskState = TaskState.Error;
        //        taskData.Result = new Dictionary<string, object>
        //        {
        //            { "ErrorMessage", e.Message }, 
        //            { "StackTrace", e.StackTrace }
        //        };
        //    }
        //}

        public static TaskData GetTask(int id)
        {
            return tasks[id];
        }

        public static Dictionary<int, TaskData> Tasks
        {
            get { return tasks; }
        }

    }
}