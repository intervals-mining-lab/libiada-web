﻿namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Bio.IO.GenBank;

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
        /// The check sequence attributes.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        /// <param name="localSubsequence">
        /// The local subsequence.
        /// </param>
        /// <param name="complement">
        /// The complement.
        /// </param>
        /// <param name="complementJoin">
        /// The complement join.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        public void CheckSequenceAttributes(FeatureItem feature, Subsequence localSubsequence, ref bool complement, ref bool complementJoin, ref bool partial)
        {
            var localAttributes = localSubsequence.SequenceAttribute.ToArray()
                                                  .GroupBy(a => a.AttributeId)
                                                  .ToDictionary(a => a.Key, a => a.ToArray());

            foreach (var qualifier in feature.Qualifiers)
            {
                switch (qualifier.Key)
                {
                    case "translation":
                        continue;
                }

                var attributeId = attributeRepository.GetAttributeByName(qualifier.Key).Id;

                var equalAttributes = localAttributes[attributeId].ToDictionary(a => a.Value);

                var cleanedValue = CleanAndJoinAttributeValues(qualifier.Value);

                SequenceAttribute value;
                if (equalAttributes.TryGetValue(cleanedValue, out value))
                {
                    localSubsequence.SequenceAttribute.Remove(value);
                }
            }

            CheckBoolAttribute(ref complement, Aliases.Attribute.Complement, localSubsequence);
            CheckBoolAttribute(ref complementJoin, Aliases.Attribute.ComplementJoin, localSubsequence);
            CheckBoolAttribute(ref partial, Aliases.Attribute.Partial, localSubsequence);
        }

        /// <summary>
        /// The get attributes.
        /// </summary>
        /// <param name="subsequenceIds">
        /// The subsequences ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:Dictionary{Int64, String[]}"/>.
        /// </returns>
        public Dictionary<long, string[]> GetAttributes(IEnumerable<long> subsequenceIds)
        {
            return db.SequenceAttribute.Where(sa => subsequenceIds.Contains(sa.SequenceId))
                                       .Include(sa => sa.Attribute)
                                       .Select(sa => new
                                                     {
                                                         sa.SequenceId,
                                                         Text = sa.Attribute.Name + (sa.Value == string.Empty ? string.Empty : " = " + sa.Value)
                                                     })
                                       .ToArray()
                                       .GroupBy(sa => sa.SequenceId)
                                       .ToDictionary(sa => sa.Key, sa => sa.Select(s => s.Text).ToArray());
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
        public List<SequenceAttribute> CreateSubsequenceAttributes(Dictionary<string, List<string>> qualifiers, bool complement, bool complementJoin, Subsequence subsequence)
        {
            var result = new List<SequenceAttribute>();

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

                    result.Add(CreateSequenceAttribute(qualifier.Key, CleanAndJoinAttributeValues(qualifier.Value), subsequence.Id));
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
        private SequenceAttribute CreateSequenceAttribute(string attributeName, string attributeValue, long sequenceId)
        {
            var attributeId = attributeRepository.GetAttributeByName(attributeName).Id;

            var subsequenceAttribute = new SequenceAttribute
            {
                AttributeId = attributeId,
                SequenceId = sequenceId,
                Value = attributeValue
            };

            return subsequenceAttribute;
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
        /// <returns>
        /// The <see cref="SequenceAttribute"/>.
        /// </returns>
        private SequenceAttribute CreateSequenceAttribute(int attributeId, string attributeValue, long sequenceId)
        {
            var subsequenceAttribute = new SequenceAttribute
            {
                AttributeId = attributeId,
                SequenceId = sequenceId,
                Value = attributeValue
            };

            return subsequenceAttribute;
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
        /// <returns>
        /// The <see cref="SequenceAttribute"/>.
        /// </returns>
        private SequenceAttribute CreateSequenceAttribute(int attributeId, long sequenceId)
        {
            return CreateSequenceAttribute(attributeId, string.Empty, sequenceId);
        }

        /// <summary>
        /// The check bool attribute.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="attributeId">
        /// The attribute id.
        /// </param>
        /// <param name="localSubsequence">
        /// The local subsequence.
        /// </param>
        private void CheckBoolAttribute(ref bool value, int attributeId, Subsequence localSubsequence)
        {
            if (value && localSubsequence.SequenceAttribute.Any(a => a.AttributeId == attributeId))
            {
                localSubsequence.SequenceAttribute.Remove(localSubsequence.SequenceAttribute.Single(a => a.AttributeId == attributeId));
                value = false;
            }
        }

        /// <summary>
        /// The clean and join attribute values.
        /// </summary>
        /// <param name="attributeValues">
        /// The attribute values.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string CleanAndJoinAttributeValues(List<string> attributeValues)
        {
            var attributeValue = string.Join("    ", attributeValues);

            return attributeValue.Replace("\"", string.Empty).Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");
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
                result.Add(CreateSequenceAttribute(Aliases.Attribute.Complement, subsequence.Id));

                if (complementJoin)
                {
                    result.Add(CreateSequenceAttribute(Aliases.Attribute.ComplementJoin, subsequence.Id));
                }
            }

            if (subsequence.Partial)
            {
                result.Add(CreateSequenceAttribute(Aliases.Attribute.Partial, subsequence.Id));
            }

            return result;
        }
    }
}
