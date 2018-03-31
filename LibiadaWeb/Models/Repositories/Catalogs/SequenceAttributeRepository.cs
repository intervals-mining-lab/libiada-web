namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Models.CalculatorsData;

    using Attribute = LibiadaWeb.Attribute;

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
            attributeRepository = new AttributeRepository();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// The get attributes.
        /// </summary>
        /// <param name="subsequenceIds">
        /// The subsequences ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:Dictionary{Int64, AttributeValue[]}"/>.
        /// </returns>
        public Dictionary<long, AttributeValue[]> GetAttributes(IEnumerable<long> subsequenceIds)
        {
            return db.SequenceAttribute.Where(sa => subsequenceIds.Contains(sa.SequenceId))
                                       .Select(sa => new
                                                     {
                                                         sa.SequenceId,
                                                         sa.Attribute,
                                                         sa.Value
                                                     })
                                       .ToArray()
                                       .GroupBy(sa => sa.SequenceId)
                                       .ToDictionary(sa => sa.Key, sa => sa.Select(av => new AttributeValue((byte)av.Attribute, av.Value)).ToArray());
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
        /// <returns>
        /// The <see cref="List{SequenceAttribute}"/>.
        /// </returns>
        public List<SequenceAttribute> Create(Dictionary<string, List<string>> qualifiers, bool complement, bool complementJoin, Subsequence subsequence)
        {
            var result = new List<SequenceAttribute>(qualifiers.Count);

            foreach ((string key, List<string> values) in qualifiers)
            {
                foreach (string value in values)
                {
                    if (key == "translation")
                    {
                        break;
                    }

                    if (key == "protein_id")
                    {
                        string remoteId = CleanAttributeValue(value);

                        if (!string.IsNullOrEmpty(subsequence.RemoteId) && subsequence.RemoteId != remoteId)
                        {
                            throw new Exception($"Several remote ids in one subsequence. First {subsequence.RemoteId} Second {remoteId}");
                        }

                        subsequence.RemoteId = remoteId;
                    }

                    result.Add(Create(key, CleanAttributeValue(value), subsequence.Id));
                }
            }

            result.AddRange(CreateComplementJoinPartialAttributes(complement, complementJoin, subsequence));

            return result;
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
        /// <returns>
        /// The <see cref="SequenceAttribute"/>.
        /// </returns>
        private SequenceAttribute Create(string attributeName, string attributeValue, long sequenceId)
        {
            Attribute attribute = attributeRepository.GetAttributeByName(attributeName);
            return Create(attribute, attributeValue, sequenceId);
        }

        /// <summary>
        /// The create sequence attribute.
        /// </summary>
        /// <param name="attribute">
        /// The attribute type.
        /// </param>
        /// <param name="attributeValue">
        /// The attribute value.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="SequenceAttribute"/>.
        /// </returns>
        private SequenceAttribute Create(Attribute attribute, string attributeValue, long sequenceId)
        {
            return new SequenceAttribute
            {
                Attribute = attribute,
                SequenceId = sequenceId,
                Value = attributeValue
            };
        }

        /// <summary>
        /// The create sequence attribute.
        /// </summary>
        /// <param name="attribute">
        /// The attribute type.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="SequenceAttribute"/>.
        /// </returns>
        private SequenceAttribute CreateSequenceAttribute(Attribute attribute, long sequenceId)
        {
            return Create(attribute, string.Empty, sequenceId);
        }

        /// <summary>
        /// Cleans attribute value.
        /// </summary>
        /// <param name="attributeValue">
        /// The attribute value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string CleanAttributeValue(string attributeValue)
        {
            return attributeValue.Replace("\"", string.Empty)
                                 .Replace("\n", " ")
                                 .Replace("\r", " ")
                                 .Replace("\t", " ")
                                 .Replace("  ", " ");
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
        /// <returns>
        /// The <see cref="List{SequenceAttribute}"/>.
        /// </returns>
        private List<SequenceAttribute> CreateComplementJoinPartialAttributes(bool complement, bool complementJoin, Subsequence subsequence)
        {
            var result = new List<SequenceAttribute>();
            if (complement)
            {
                result.Add(CreateSequenceAttribute(Attribute.Complement, subsequence.Id));

                if (complementJoin)
                {
                    result.Add(CreateSequenceAttribute(Attribute.ComplementJoin, subsequence.Id));
                }
            }

            if (subsequence.Partial)
            {
                result.Add(CreateSequenceAttribute(Attribute.Partial, subsequence.Id));
            }

            return result;
        }
    }
}
