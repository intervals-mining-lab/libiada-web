namespace LibiadaWeb.Migrations
{
    using System.Data.Entity.Migrations;

    using LibiadaWeb.Models.Account;

    /// <summary>
    /// The configuration.
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        /// <summary>
        /// The seed.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        protected override void Seed(ApplicationDbContext context)
        {
        }
    }
}
