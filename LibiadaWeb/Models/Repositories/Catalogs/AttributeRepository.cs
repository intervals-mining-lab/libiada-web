namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Linq;

    using LibiadaWeb;

    /// <summary>
    /// The attribute repository.
    /// </summary>
    public class AttributeRepository : IAttributeRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public AttributeRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// Gets or creates attribute by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="Attribute"/>.
        /// </returns>
        public Attribute GetOrCreateAttributeByName(string name)
        {
            var allAttributes = db.Attribute;

           Attribute attribute;

           if (allAttributes.Any(a => a.Name == name))
            {
                attribute = allAttributes.Single(a => a.Name == name);
            }
            else
            {
                attribute = new Attribute { Name = name };
                db.Attribute.Add(attribute);
            }

            return attribute;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }
    }
}