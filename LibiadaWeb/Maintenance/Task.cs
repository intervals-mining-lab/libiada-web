using System;
using System.Collections.Generic;

namespace LibiadaWeb.Maintenance
{
    public class Task
    {
        public TaskData TaskData;

        public Dictionary<string, object> Result;

        public Func<Dictionary<string, object>> Action;

        public string ControllerName;

        public Task(Func<Dictionary<string, object>> action, int id)
        {
            Action = action;
            TaskData = new TaskData(id);
        }
    }
}