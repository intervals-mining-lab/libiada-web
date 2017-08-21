namespace LibiadaWeb.Helpers
{
    using System.Web;

    using Microsoft.AspNet.Identity;

    /// <summary>
    /// Envelop for some user methods.
    /// </summary>
    public static class AccountHelper
    {
        /// <summary>
        /// Gets id of current user.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static int GetUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId<int>();
        }

        /// <summary>
        /// Checks if user has admin role.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsAdmin()
        {
            if (HttpContext.Current == null)
            {
                return false;
            }

            return HttpContext.Current.User.IsInRole("Admin");
        }
    }
}
