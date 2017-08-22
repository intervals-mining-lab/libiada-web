namespace LibiadaWeb.Helpers
{
    using System.Web;

    using LibiadaWeb.Models.Account;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;

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
        /// Finds user name by it's id.
        /// </summary>
        /// <param name="id">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetUserNameById(int id)
        {
            return GetUserById(id).UserName;
        }

        /// <summary>
        /// Finds user by it's id.
        /// </summary>
        /// <param name="id">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="ApplicationUser"/>.
        /// </returns>
        public static ApplicationUser GetUserById(int id)
        {
            return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(id);
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
