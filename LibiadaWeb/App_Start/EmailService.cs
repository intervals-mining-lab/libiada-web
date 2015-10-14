namespace LibiadaWeb
{
    using System;
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
            var configurationFile = WebConfigurationManager.OpenWebConfiguration("~/web.config");
            var mailSettings = configurationFile.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
            
            if (mailSettings == null)
            {
                throw new Exception("Mail Settings not found");
            }

            int port = mailSettings.Smtp.Network.Port;
            string host = mailSettings.Smtp.Network.Host;
            string password = mailSettings.Smtp.Network.Password;
            string username = mailSettings.Smtp.Network.UserName;
            string from = mailSettings.Smtp.Network.UserName;

            using (var mailMessage = new MailMessage(from, message.Destination, message.Subject, message.Body))
            {
                using (var smtp = new SmtpClient(host, port))
                {
                    smtp.Credentials = new NetworkCredential(username, password);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.ServicePoint.MaxIdleTime = 1;
                    smtp.EnableSsl = true;

                    mailMessage.IsBodyHtml = true;
                    smtp.Send(mailMessage);
                }
            }

            return Task.FromResult(0);
        }
    }
}
