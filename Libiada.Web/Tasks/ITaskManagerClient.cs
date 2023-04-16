namespace Libiada.Web.Tasks
{
    public interface ITaskManagerClient
    {
        System.Threading.Tasks.Task TaskEvent(TaskEvent @event, object data);
    }
}
