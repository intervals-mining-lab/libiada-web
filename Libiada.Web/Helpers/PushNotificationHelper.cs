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
    private readonly string privateKey;
    private readonly string publicKey;
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;

    public PushNotificationHelper(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, IConfiguration configuration)
    {
        publicKey = configuration["PublicVapidKey"] ?? throw new Exception($"PublicVapidKey is not found in confiuguration.");
        privateKey = configuration["PrivateVapidKey"] ?? throw new Exception($"PrivateVapidKey is not found in confiuguration.");
        this.dbFactory = dbFactory;
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
        using var db = dbFactory.CreateDbContext();
        var subscribers = db.AspNetPushNotificationSubscribers.Where(s => s.UserId == userId);

        if (subscribers.Any())
        {
            foreach (var subscriber in subscribers)
            {
                string endpoint = subscriber.Endpoint;
                string p256dh = subscriber.P256dh;
                string auth = subscriber.Auth;
                string payload = JsonConvert.SerializeObject(data);

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
