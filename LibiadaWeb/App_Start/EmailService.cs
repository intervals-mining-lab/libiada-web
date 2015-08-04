namespace LibiadaWeb
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Net.Configuration;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using Microsoft.AspNet.Identity;

    /// <summary>
    /// The email service.
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        /// <summary>
        /// The send async.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.

            var configurationFile = WebConfigurationManager.OpenWebConfiguration("~/web.config");
            var mailSettings = configurationFile.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
            
            if (mailSettings == null)
            {
                throw new Exception("Mail Settings not found");
            }

            int port = mailSettings.Smtp.Network.Port;
            string host = mailSettings.Smtp.Network.Host;
            string password = ConfigurationManager.AppSettings["password"];
            string username = ConfigurationManager.AppSettings["userName"];
            string from = ConfigurationManager.AppSettings["userName"];
            
            //using (var mailMessage = new MailMessage(from, message.Destination, message.Subject, message.Body))
            //{
            //    using (var smtp = new SmtpClient(host, port))
            //    {
            //        smtp.Credentials = new NetworkCredential(username, password);
            //        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            //        smtp.ServicePoint.MaxIdleTime = 1;
            //        smtp.EnableSsl = true;

            //        smtp.Send(mailMessage);
            //    }
            //}

            return Task.FromResult(0);
        }
    }
}
