namespace LibiadaWeb.Models.Account
{
    using System.Web;

    using Microsoft.AspNet.Identity;

    /// <summary>
    /// Envelop for some user methods.
    /// </summary>
    public static class UserHelper
    {
        /// <summary>
        /// Gets id of current user.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        /// <summary>
        /// Checks if user has admin role.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsAdmin()
        {
            return HttpContext.Current.User.IsInRole("Admin");
        }
    }
}