namespace Libiada.Web.Tasks
{
    public interface ITaskManagerClient
    {
        System.Threading.Tasks.Task TaskEvent(string @event, object data);
    }
}
