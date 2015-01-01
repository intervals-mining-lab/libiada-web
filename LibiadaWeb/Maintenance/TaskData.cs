using System.Collections.Generic;
using Antlr.Runtime.Misc;

namespace LibiadaWeb.Maintenance
{
    public class TaskData
    {

        public TaskData(Action action)
        {
            Action = action;
        }

        public TaskState TaskState;
        
        public Dictionary<string, object> Result;

        public Action Action;

    }
}