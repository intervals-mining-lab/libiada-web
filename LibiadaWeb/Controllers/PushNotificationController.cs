using LibiadaWeb.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{
    public class PushNotificationController : Controller
    {
        /// <summary>
        /// Database context.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes database context.
        /// </summary>
        public PushNotificationController()
        {
            db = new LibiadaWebEntities();
        }

        /// <summary>
        /// Subscribes a user to receive push notifications.
        /// </summary>
        /// <param name="subscriberData">
        /// Subscriber data that contains endpoint, pubic key and private key. 
        /// </param>
        [System.Web.Mvc.Authorize]
        [System.Web.Mvc.HttpPost]
        public void Subscribe([FromBody]AspNetPushNotificationSubscriber subscriberData)
        {
            try
            {
                var subscriber = new AspNetPushNotificationSubscriber
                {
                    Auth = subscriberData.Auth,
                    P256dh = subscriberData.P256dh,
                    Endpoint = subscriberData.Endpoint,
                    UserId = Convert.ToInt32(AccountHelper.GetUserId())
                };
                db.AspNetPushNotificationSubscribers.Add(subscriber);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Subscribe error: {e.Message}");
            }
        }

        /// <summary>
        /// Unsubscribes a user to not receive push notifications.
        /// </summary>
        /// <param name="subscriberData">
        /// Subscriber data that contains endpoint.
        /// </param>
        [System.Web.Mvc.Authorize]
        [System.Web.Mvc.HttpDelete]
        public void Unsubscribe([FromBody]AspNetPushNotificationSubscriber subscriberData)
        {
            try
            {
                var endpoint = subscriberData.Endpoint;
                var subscriber = db.AspNetPushNotificationSubscribers.Single(s => s.Endpoint == endpoint);
                db.AspNetPushNotificationSubscribers.Remove(subscriber);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unsubscribe error: {e.Message}");
            }
        }
    }
}