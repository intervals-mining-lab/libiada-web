using System;

namespace LibiadaWeb.Maintenance
{
    public class TaskData
    {

        public TaskData()
        {
            Created = DateTime.Now;
        }

        public DateTime Created;

        public TaskState TaskState;
        
        public string ActionName;

    }
}