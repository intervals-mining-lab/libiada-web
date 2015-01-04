using System;
using System.Collections.Generic;
using System.Threading;

namespace LibiadaWeb.Maintenance
{
    public class Task
    {
        public TaskData TaskData;

        public Dictionary<string, object> Result;

        public Func<Dictionary<string, object>> Action;

        public string ControllerName;

        public Thread Thread;

        public Task(Func<Dictionary<string, object>> action, int id)
        {
            Action = action;
            TaskData = new TaskData(id);
        }
    }
}