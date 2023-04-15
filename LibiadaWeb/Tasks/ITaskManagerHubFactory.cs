namespace LibiadaWeb.Tasks
{
    public interface ITaskManagerHubFactory
    {
        TaskManagerHub Create(ITaskManager taskManager);
    }
}
