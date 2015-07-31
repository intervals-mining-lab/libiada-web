namespace LibiadaWeb.Models.Account
{
    using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// The libiada user store.
    /// </summary>
    public class LibiadaUserStore : UserStore<ApplicationUser, LibiadaRole, int, LibiadaUserLogin, LibiadaUserRole, LibiadaUserClaim>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibiadaUserStore"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public LibiadaUserStore(ApplicationDbContext context) : base(context)
        {
        }
    }
}
