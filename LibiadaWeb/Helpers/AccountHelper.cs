namespace LibiadaWeb.Helpers
{
    using System.Security.Claims;
    using System.Security.Principal;

    /// <summary>
    /// Envelop for some user methods.
    /// </summary>
    public static class AccountHelper
    {
        /// <summary>
        /// Gets id of the given user.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetUserId(this IPrincipal principal)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim.Value);
        }

        /// <summary>
        /// Finds user name by its id.
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
            using (var db = new LibiadaDatabaseEntities())
            {
                return db.AspNetUsers.Find(id);
            }
        }
    }
}
