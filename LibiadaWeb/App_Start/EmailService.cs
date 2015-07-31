namespace LibiadaWeb
{
    using System.Threading.Tasks;

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
            return Task.FromResult(0);
        }
    }
}
