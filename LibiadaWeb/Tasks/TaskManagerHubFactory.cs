namespace LibiadaWeb.Tasks
{
    public class TaskManagerHubFactory : ITaskManagerHubFactory
    {
        public TaskManagerHub Create(ITaskManager taskManager)
        {
            return new TaskManagerHub(taskManager);
        }
    }
}
