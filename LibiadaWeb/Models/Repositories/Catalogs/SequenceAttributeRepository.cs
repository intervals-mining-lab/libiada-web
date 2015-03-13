namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The sequence attribute.
    /// </summary>
    public class SequenceAttributeRepository : ISequenceAttributeRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The attribute repository.
        /// </summary>
        private readonly AttributeRepository attributeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceAttributeRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public SequenceAttributeRepository(LibiadaWebEntities db)
        {
            this.db = db;
            attributeRepository = new AttributeRepository(db);
        }

        /// <summary>
        /// Creates and adds to db sequence attribute and attribute.
        /// </summary>
        /// <param name="qualifier">
        /// The qualifier.
        /// </param>
        /// <param name="fragment">
        /// The fragment.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if qualifier has more than one value.
        /// </exception>
        public void CreateSequenceAttribute(KeyValuePair<string,List<string>> qualifier, Fragment fragment)
        {
            if (qualifier.Value.Count > 1)
            {
                throw new Exception("Qualifier contains more than 1 value. Qualifier=" + qualifier.Key);
            }

            if (qualifier.Value.Count != 0)
            {
                switch (qualifier.Key)
                {
                    case "translation":
                        return;
                    case "db_xref":
                        fragment.WebApiId = int.Parse(qualifier.Value[0].Substring(3));
                        break;
                    case "codon_start":
                        if (qualifier.Value[0] != "1")
                        {
                            throw new Exception("Codon start is not 1. value = " + qualifier.Value[0]);
                        }

                        break;
                }

                var attribute = attributeRepository.GetOrCreateAttributeByName(qualifier.Key);

                var fragmentAttribute = new SequenceAttribute
                {
                    Attribute = attribute,
                    Fragment = fragment,
                    Value = qualifier.Value[0]
                };

                db.SequenceAttribute.Add(fragmentAttribute);
            }
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