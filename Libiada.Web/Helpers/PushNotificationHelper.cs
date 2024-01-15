namespace Libiada.Web.Helpers;

using Newtonsoft.Json;
using System.Diagnostics;

using WebPush;

public class PushNotificationHelper : IPushNotificationHelper
{
    /// <summary>
    /// The subject.
    /// </summary>
    const string subject = @"https://foarlab.org/";
    private readonly LibiadaDatabaseEntities db;
    private readonly string privateKey;
    private readonly string publicKey;

    public PushNotificationHelper(ILibiadaDatabaseEntitiesFactory dbFactory, IConfiguration configuration)
    {
        this.db = dbFactory.CreateDbContext();
        publicKey = configuration["PublicVapidKey"] ?? throw new Exception($"PublicVapidKey is not found in confiuguration.");
        privateKey = configuration["PrivateVapidKey"] ?? throw new Exception($"PrivateVapidKey is not found in confiuguration.");
    }

    /// <summary>
    /// Send push notification to subscribers.
    /// </summary>
    /// <param name="userId">
    /// User id for sending the push notification.
    /// </param>
    /// <param name="data">
    /// Data dictionary containing push notification elements.
    /// </param>
    public void Send(int userId, Dictionary<string, string> data)
    {
        var subscribers = db.AspNetPushNotificationSubscribers.Where(s => s.UserId == userId);

        if (subscribers.Any())
        {
            foreach (var subscriber in subscribers)
            {
                var endpoint = subscriber.Endpoint;
                var p256dh = subscriber.P256dh;
                var auth = subscriber.Auth;
                var payload = JsonConvert.SerializeObject(data);

                var subscription = new PushSubscription(endpoint, p256dh, auth);
                var options = new Dictionary<string, object>()
                {
                    {"TTL", 3600 },
                    {"vapidDetails", new VapidDetails(subject, publicKey, privateKey) }
                };

                try
                {
                    using var webPushClient = new WebPushClient();
                    webPushClient.SendNotification(subscription, payload, options);
                }
                catch (WebPushException exception)
                {
                    Debug.WriteLine($"Failed to send push notification. Http STATUS code: {exception.StatusCode}. {exception.Message}");
                }
            }
        }
    }
}
