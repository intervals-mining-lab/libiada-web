namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaWeb.Extensions;

    using Attribute = LibiadaWeb.Attribute;

    /// <summary>
    /// The attribute repository.
    /// </summary>
    public class AttributeRepository : IAttributeRepository
    {
        /// <summary>
        /// The attributes.
        /// </summary>
        private readonly Attribute[] attributes;

        /// <summary>
        /// The attributes dictionary.
        /// </summary>
        private readonly Dictionary<string, Attribute> attributesDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRepository"/> class.
        /// </summary>
        public AttributeRepository()
        {
            attributes = EnumExtensions.ToArray<Attribute>();
            attributesDictionary = attributes.ToList().ToDictionary(a => a.GetDisplayValue());
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
            Attribute value;
            if (attributesDictionary.TryGetValue(name, out value))
            {
                return value;
            }
            else
            {
                throw new Exception("Unknown attribute: " + name);
            }
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
            if (attributes.Any(a => (byte)a == id))
            {
                return attributes.Single(a => (byte)a == id).GetDisplayValue();
            }
            else
            {
                throw new Exception("Unknown attribute: " + id);
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
