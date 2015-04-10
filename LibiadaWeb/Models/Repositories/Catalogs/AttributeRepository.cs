namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaWeb;

    using Attribute = LibiadaWeb.Attribute;

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
        /// The attributes.
        /// </summary>
        private readonly List<Attribute> attributes; 

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public AttributeRepository(LibiadaWebEntities db)
        {
            this.db = db;
            attributes = db.Attribute.ToList();
        }

        /// <summary>
        /// Gets attribute by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="Attribute"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if attribute with given name is not found.
        /// </exception>
        public Attribute GetAttributeByName(string name)
        {
            if (attributes.All(a => a.Name != name))
            {
                throw new Exception("Unknown attribute: " + name);
            }

            return attributes.Single(a => a.Name == name);
        }

        /// <summary>
        /// Gets attribute name by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Attribute"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if attribute with given id is not found.
        /// </exception>
        public string GetAttributeNameById(int id)
        {
            if (attributes.All(a => a.Id != id))
            {
                throw new Exception("Unknown attribute: " + id);
            }

            return attributes.Single(a => a.Id == id).Name;
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
