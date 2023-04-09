namespace LibiadaWeb.Tasks
{
    public interface ITaskManagerClient
    {
        System.Threading.Tasks.Task TaskEvent(TaskEvent @event, object data);
    }
}
