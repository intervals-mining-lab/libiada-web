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
            return GetUserById(id)?.UserName;
        }

        /// <summary>
        /// Checks if user has admin role.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsAdmin()
        {
            return HttpContext.Current != null && HttpContext.Current.User.IsInRole("Admin");
        }

        /// <summary>
        /// Finds user by it's id.
        /// </summary>
        /// <param name="id">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="Models.Account.ApplicationUser"/>.
        /// </returns>
        private static AspNetUser GetUserById(int id)
        {
            using (var db = new LibiadaWebEntities())
            {
                return db.AspNetUsers.Find(id);
            }
        }
    }
}
