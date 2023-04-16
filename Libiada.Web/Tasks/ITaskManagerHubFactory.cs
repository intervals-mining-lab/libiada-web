namespace Libiada.Web.Tasks
{
    public interface ITaskManagerHubFactory
    {
        TaskManagerHub Create(ITaskManager taskManager);
    }
}
