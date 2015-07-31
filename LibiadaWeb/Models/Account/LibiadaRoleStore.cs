namespace LibiadaWeb.Models.Account
{
    using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// The libiada role store.
    /// </summary>
    public class LibiadaRoleStore : RoleStore<LibiadaRole, int, LibiadaUserRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibiadaRoleStore"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public LibiadaRoleStore(ApplicationDbContext context) : base(context)
        {
        }
    }
}
