using Libiada.Database.Tasks;

namespace Libiada.Web.Tasks
{
    public interface ITaskManager
    {
        long CreateTask(Func<Dictionary<string, string>> action, TaskType taskType);
        IEnumerable<TaskData> DeleteAllTasks();
        TaskData DeleteTask(long id);
        IEnumerable<TaskData> DeleteTasksWithState(TaskState taskState);
        Task GetTask(long id);
        string GetTaskData(long id, string key = "data");
        IEnumerable<TaskData> GetTasksData();
    }
}