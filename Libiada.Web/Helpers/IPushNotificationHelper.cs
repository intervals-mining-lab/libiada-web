
namespace Libiada.Web.Helpers
{
    public interface IPushNotificationHelper
    {
        void Send(int userId, Dictionary<string, string> data);
    }
}