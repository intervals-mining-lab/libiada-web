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
        /// <param name="qualifiers">
        /// The attributes to add.
        /// </param>
        /// <param name="complement">
        /// Complement flag.
        /// </param>
        /// <param name="complementJoin">
        /// Complement join flag.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if qualifier has more than one value.
        /// </exception>
        public void CreateSequenceAttributes(Dictionary<string, List<string>> qualifiers, bool complement, bool complementJoin, Subsequence subsequence)
        {
            foreach (var qualifier in qualifiers)
            {
                if (qualifier.Value.Count > 1)
                {
                    throw new Exception("Qualifier contains more than 1 value. Qualifier=" + qualifier.Key);
                }

                if (qualifier.Value.Count == 1)
                {
                    switch (qualifier.Key)
                    {
                        case "translation":
                            continue;
                        case "db_xref":
                            subsequence.WebApiId = int.Parse(qualifier.Value[0].Substring(3));
                            break;
                        case "codon_start":
                            if (qualifier.Value[0] != "1")
                            {
                                throw new Exception("Codon start is not 1. value = " + qualifier.Value[0]);
                            }

                            break;
                    }

                    CreateSequenceAttribute(qualifier.Key, qualifier.Value[0], subsequence);
                }
            }

            CreateComplementJoinAttribute(complement, complementJoin, subsequence);
        }

        /// <summary>
        /// The create sequence attribute.
        /// </summary>
        /// <param name="attributeName">
        /// The attribute name.
        /// </param>
        /// <param name="attributeValue">
        /// The attribute value.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        public void CreateSequenceAttribute(string attributeName, string attributeValue, Subsequence subsequence)
        {
            var attribute = attributeRepository.GetOrCreateAttributeByName(attributeName);

            var subsequenceAttribute = new SequenceAttribute
            {
                Attribute = attribute,
                Subsequence = subsequence,
                Value = attributeValue
            };

            db.SequenceAttribute.Add(subsequenceAttribute);
        }

        /// <summary>
        /// The create sequence attribute.
        /// </summary>
        /// <param name="attributeName">
        /// The attribute name.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        public void CreateSequenceAttribute(string attributeName, Subsequence subsequence)
        {
            CreateSequenceAttribute(attributeName, string.Empty, subsequence);
        }

        /// <summary>
        /// The create complement join attribute.
        /// </summary>
        /// <param name="complement">
        /// The complement.
        /// </param>
        /// <param name="complementJoin">
        /// The complement join.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        public void CreateComplementJoinAttribute(bool complement, bool complementJoin, Subsequence subsequence)
        {
            if (complement)
            {
                CreateSequenceAttribute("complement", subsequence);

                if (complementJoin)
                {
                    CreateSequenceAttribute("complementJoin", subsequence);
                }
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
