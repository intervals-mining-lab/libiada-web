namespace LibiadaWeb.Models.Account
{
    using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// The libiada role.
    /// </summary>
    public class LibiadaRole : IdentityRole<int, LibiadaUserRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibiadaRole"/> class.
        /// </summary>
        public LibiadaRole()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibiadaRole"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public LibiadaRole(string name)
        {
            Name = name;
        }
    }
}
