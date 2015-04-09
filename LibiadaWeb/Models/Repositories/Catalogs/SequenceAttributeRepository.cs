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
        /// Creates and adds to db subsequence attributes.
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
        public void CreateSubsequenceAttributes(Dictionary<string, List<string>> qualifiers, bool complement, bool complementJoin, Subsequence subsequence)
        {
            foreach (var qualifier in qualifiers)
            {
                if (qualifier.Value.Count == 1)
                {
                    switch (qualifier.Key)
                    {
                        case "translation":
                            continue;
                        case "db_xref":
                            foreach (var value in qualifier.Value)
                            {
                                if (Regex.IsMatch(value, "^\"GI:\\d+\"$"))
                                {
                                    if (subsequence.WebApiId != null)
                                    {
                                        throw new Exception("Several web api ids in one subsequence. First " + subsequence.Id + "Second " + value);
                                    }

                                    subsequence.WebApiId = int.Parse(Regex.Replace(value, @"[^\d]", string.Empty));
                                }
                            }
                            
                            break;
                    }

                    CreateSequenceAttribute(qualifier.Key, string.Join("    ", qualifier.Value), subsequence.Id);
                }
            }

            CreateComplementJoinPartialAttributes(complement, complementJoin, subsequence);
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
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        public void CreateSequenceAttribute(string attributeName, string attributeValue, long sequenceId)
        {
            var attributeId = attributeRepository.GetAttributeByName(attributeName).Id;

            var subsequenceAttribute = new SequenceAttribute
            {
                AttributeId = attributeId,
                SequenceId = sequenceId,
                Value = attributeValue.Replace("\"", string.Empty).Replace("\n", " ").Replace("\r", " ").Replace("\t", " ")
            };

            db.SequenceAttribute.Add(subsequenceAttribute);
        }

        /// <summary>
        /// The create sequence attribute.
        /// </summary>
        /// <param name="attributeName">
        /// The attribute name.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        public void CreateSequenceAttribute(string attributeName, long sequenceId)
        {
            CreateSequenceAttribute(attributeName, string.Empty, sequenceId);
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
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        public void CreateSequenceAttribute(int attributeId, string attributeValue, long sequenceId)
        {
            var subsequenceAttribute = new SequenceAttribute
            {
                AttributeId = attributeId,
                SequenceId = sequenceId,
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
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        public void CreateSequenceAttribute(int attributeId, long sequenceId)
        {
            CreateSequenceAttribute(attributeId, string.Empty, sequenceId);
        }

        /// <summary>
        /// Creates complement, join and partial attributes.
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
        private void CreateComplementJoinPartialAttributes(bool complement, bool complementJoin, Subsequence subsequence)
        {
            if (complement)
            {
                CreateSequenceAttribute(Aliases.Attribute.Complement, subsequence.Id);

                if (complementJoin)
                {
                    CreateSequenceAttribute(Aliases.Attribute.ComplementJoin, subsequence.Id);
                }
            }

            if (subsequence.Partial)
            {
                CreateSequenceAttribute(Aliases.Attribute.Partial, subsequence.Id);
            }
        }
    }
}
