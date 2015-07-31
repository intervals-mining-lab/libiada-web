namespace LibiadaWeb.Models.Account
{
    using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// The application db context.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, LibiadaRole, int, LibiadaUserLogin, LibiadaUserRole, LibiadaUserClaim> 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        public ApplicationDbContext() : base("Account")
        {
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ApplicationDbContext"/>.
        /// </returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
