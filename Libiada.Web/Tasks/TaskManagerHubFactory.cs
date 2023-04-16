namespace Libiada.Web.Tasks
{
    public class TaskManagerHubFactory : ITaskManagerHubFactory
    {
        public TaskManagerHub Create(ITaskManager taskManager) => new TaskManagerHub(taskManager);
    }
}
