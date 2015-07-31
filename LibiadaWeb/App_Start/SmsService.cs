namespace LibiadaWeb
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.Identity;

    /// <summary>
    /// The sms service.
    /// </summary>
    public class SmsService : IIdentityMessageService
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
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
