namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Extensions;

    using Attribute = LibiadaWeb.Attribute;

    /// <summary>
    /// The attribute repository.
    /// </summary>
    public class AttributeRepository
    {
        /// <summary>
        /// The attributes dictionary.
        /// </summary>
        private readonly Dictionary<string, Attribute> attributesDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRepository"/> class.
        /// </summary>
        public AttributeRepository()
        {
            Attribute[] attributes = EnumExtensions.ToArray<Attribute>();
            attributesDictionary = attributes.ToDictionary(a => a.GetDisplayValue());
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
            if (attributesDictionary.TryGetValue(name, out Attribute value))
            {
                return value;
            }
            else
            {
                throw new Exception($"Unknown attribute: {name}");
            }
        }
    }
}
