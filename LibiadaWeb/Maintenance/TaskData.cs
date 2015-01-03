using System;

namespace LibiadaWeb.Maintenance
{
    public class TaskData
    {
        public DateTime Created;

        public TaskState TaskState;
        
        public string ActionName;

        public int Id;

        public TaskData(int id)
        {
            Created = DateTime.Now;
            Id = id;
        }
    }
}