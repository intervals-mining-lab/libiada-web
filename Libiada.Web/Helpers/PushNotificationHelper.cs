﻿namespace Libiada.Web.Helpers
{
    using Newtonsoft.Json;

    using System.Configuration;
    using System.Diagnostics;

    using WebPush;

    public static class PushNotificationHelper
    {
        /// <summary>
        /// The subject.
        /// </summary>
        const string subject = @"http://foarlab.org/";

        /// <summary>
        /// Send push notification to subscribers.
        /// </summary>
        /// <param name="userId">
        /// User id for sending the push notification.
        /// </param>
        /// <param name="data">
        /// Data dictionary containing push notification elements.
        /// </param>
        public static void Send(LibiadaDatabaseEntities db, int userId, Dictionary<string, string> data)
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
                    var options = new Dictionary<string, object>();
                    options["TTL"] = 3600;

                    var publicKey = ConfigurationManager.AppSettings["PublicVapidKey"];
                    var privateKey = ConfigurationManager.AppSettings["PrivateVapidKey"];
                    options["vapidDetails"] = new VapidDetails(subject, publicKey, privateKey);
                    var webPushClient = new WebPushClient();
                    try
                    {
                        webPushClient.SendNotification(subscription, payload, options);
                    }
                    catch (WebPushException exception)
                    {
                        Debug.WriteLine("Http STATUS code: {0}", exception.StatusCode);
                    }
                }
            }
        }
    }
}
