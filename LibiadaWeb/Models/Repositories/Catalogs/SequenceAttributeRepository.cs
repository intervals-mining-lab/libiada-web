namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text.RegularExpressions;

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
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// The get attributes.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public List<string> GetAttributes(long sequenceId)
        {
            var sequenceAttributes = db.SequenceAttribute.Where(sa => sa.SequenceId == sequenceId).Include(sa => sa.Attribute);

            return sequenceAttributes.Select(sa => sa.Attribute.Name + (sa.Value == string.Empty ? string.Empty : " = " + sa.Value)).ToList();
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
                            subsequence.WebApiId = int.Parse(Regex.Replace(qualifier.Value[0], @"[^\d]", string.Empty));
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
            var attributeId = attributeRepository.GetAttributeByName(attributeName).Id;

            var subsequenceAttribute = new SequenceAttribute
            {
                AttributeId = attributeId,
                SequenceId = subsequence.Id,
                Value = attributeValue.Replace("\"", string.Empty).Replace("\n", " ")
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
        /// The create sequence attribute.
        /// </summary>
        /// <param name="attributeId">
        /// The attribute id.
        /// </param>
        /// <param name="attributeValue">
        /// The attribute value.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        public void CreateSequenceAttribute(int attributeId, string attributeValue, Subsequence subsequence)
        {
            var subsequenceAttribute = new SequenceAttribute
            {
                AttributeId = attributeId,
                SequenceId = subsequence.Id,
                Value = attributeValue
            };

            db.SequenceAttribute.Add(subsequenceAttribute);
        }

        /// <summary>
        /// The create sequence attribute.
        /// </summary>
        /// <param name="attributeId">
        /// The attribute id.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        public void CreateSequenceAttribute(int attributeId, Subsequence subsequence)
        {
            CreateSequenceAttribute(attributeId, string.Empty, subsequence);
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
        private void CreateComplementJoinAttribute(bool complement, bool complementJoin, Subsequence subsequence)
        {
            if (complement)
            {
                CreateSequenceAttribute(Aliases.Attribute.Complement, subsequence);

                if (complementJoin)
                {
                    CreateSequenceAttribute(Aliases.Attribute.ComplementJoin, subsequence);
                }
            }
        }
    }
}
