using Libiada.Database.Tasks;

namespace LibiadaWeb.Tasks
{
    public interface ITaskManager
    {
        long CreateTask(Func<Dictionary<string, string>> action, TaskType taskType);
        void DeleteAllTasks();
        void DeleteTask(long id);
        void DeleteTasksWithState(TaskState taskState);
        Task GetTask(long id);
        string GetTaskData(long id, string key = "data");
        IEnumerable<TaskData> GetTasksData();
    }
}