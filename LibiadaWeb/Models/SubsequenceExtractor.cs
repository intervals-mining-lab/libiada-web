namespace LibiadaWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Models.Repositories.Sequences;

    using Attribute = LibiadaWeb.Attribute;

    /// <summary>
    /// The subsequence extractor.
    /// </summary>
    public class SubsequenceExtractor
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The common sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceExtractor"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public SubsequenceExtractor(LibiadaWebEntities db)
        {
            this.db = db;
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// Extracts sequences for given subsequences from database.
        /// </summary>
        /// <param name="subsequences">
        /// The subsequences.
        /// </param>
        /// <returns>
        /// The <see cref="List{Chain}"/>.
        /// </returns>
        public Dictionary<long, Chain> GetSubsequencesSequences(Subsequence[] subsequences)
        {
            long[] distinctIds = subsequences.Select(s => s.SequenceId).Distinct().ToArray();
            if (distinctIds.Length == 1)
            {
                Sequence sourceSequence = GetDotNetBioSequence(distinctIds.Single());
                var result = new Dictionary<long, Chain>();

                foreach (Subsequence subsequence in subsequences)
                {
                    Chain sequence = GetSequence(sourceSequence, subsequence);
                    result.Add(subsequence.Id, sequence);
                }

                return result;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Extracts sequence for given subsequence from database.
        /// </summary>
        /// <param name="subsequence">
        /// Subsequence to be extracted from database.
        /// </param>
        /// <returns></returns>
        public Chain GetSubsequenceSequence(Subsequence subsequence)
        {
            Sequence sourceSequence = GetDotNetBioSequence(subsequence.SequenceId);
            return GetSequence(sourceSequence, subsequence);
        }

        /// <summary>
        /// The extract sequences.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <param name="features">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="List{Subsequence}"/>.
        /// </returns>
        public Subsequence[] GetSubsequences(long sequenceId, IReadOnlyList<Feature> features)
        {
            Feature[] allFeatures = EnumExtensions.ToArray<Feature>();
            if (allFeatures.Length == features.Count)
            {
                return db.Subsequence.Where(s => s.SequenceId == sequenceId)
                    .Include(s => s.Position)
                    .Include(s => s.SequenceAttribute)
                    .ToArray();
            }

            if (allFeatures.Length - 1 == features.Count)
            {
                Feature exceptFeature = allFeatures.Except(features).Single();

                return db.Subsequence.Where(s => s.SequenceId == sequenceId && s.Feature != exceptFeature)
                    .Include(s => s.Position)
                    .Include(s => s.SequenceAttribute)
                    .ToArray();
            }

            return db.Subsequence.Where(s => s.SequenceId == sequenceId && features.Contains(s.Feature))
                                 .Include(s => s.Position)
                                 .Include(s => s.SequenceAttribute)
                                 .ToArray();
        }

        /// <summary>
        /// Extracts only filtered subsequences.
        /// </summary>
        /// <param name="sequenceId">
        /// Sequences id.
        /// </param>
        /// <param name="features">
        /// Subsequences features.
        /// </param>
        /// <param name="filters">
        /// Filters for the subsequences.
        /// Filters are applied in "OR" logic (if subsequence corresponds to any filter it is added to calculation).
        /// </param>
        /// <returns>
        /// Array of subsequences.
        /// </returns>
        public Subsequence[] GetSubsequences(long sequenceId, IReadOnlyList<Feature> features, string[] filters)
        {
            filters = filters.ConvertAll(f => f.ToLowerInvariant()).ToArray();
            var result = new List<Subsequence>();
            Subsequence[] allSubsequences = GetSubsequences(sequenceId, features);

            foreach (Subsequence subsequence in allSubsequences)
            {
                if (IsSubsequenceAttributePassesFilters(subsequence, Attribute.Product, filters)
                 || IsSubsequenceAttributePassesFilters(subsequence, Attribute.Gene, filters)
                 || IsSubsequenceAttributePassesFilters(subsequence, Attribute.LocusTag, filters))
                {
                        result.Add(subsequence);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        ///Extracts subsequence from given parent sequence.
        /// </summary>
        /// <param name="source">
        /// Parent sequence for extraction.
        /// </param>
        /// <param name="subsequence">
        /// Subsequence to be extracted from parent sequence.
        /// </param>
        /// <returns>
        /// Extracted from given position sequence as <see cref="Chain"/>.
        /// </returns>
        private Chain GetSequence(Sequence source, Subsequence subsequence)
        {
            if (subsequence.Position.Count == 0)
            {
                return GetSimpleSubsequence(source, subsequence);
            }
            else
            {
                return GetJoinedSubsequence(source, subsequence);
            }
        }

        /// <summary>
        /// Extracts .net bio <see cref="Sequence"/> from database.
        /// </summary>
        /// <param name="sequenceId">
        /// Id of the sequence to be retrieved from database.
        /// </param>
        /// <returns>
        /// Subsequence as .net bio <see cref="Sequence"/>.
        /// </returns>
        private Sequence GetDotNetBioSequence(long sequenceId)
        {
            string parentChain = commonSequenceRepository.GetString(sequenceId);
            return new Sequence(Alphabets.DNA, parentChain);
        }

        /// <summary>
        /// Checks if subsequence attribute passes filters.
        /// </summary>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        /// <param name="attribute">
        /// The attribute.
        /// </param>
        /// <param name="filters">
        /// The filters.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsSubsequenceAttributePassesFilters(Subsequence subsequence, Attribute attribute, string[] filters)
        {
            if (subsequence.SequenceAttribute.Any(sa => sa.Attribute == attribute))
            {
                string value = subsequence.SequenceAttribute.Single(sa => sa.Attribute == attribute).Value.ToLowerInvariant();
                return filters.Any(f => value.Contains(f));
            }

            return false;
        }

        /// <summary>
        /// Extracts subsequence without joins (additional positions).
        /// </summary>
        /// <param name="sourceSequence">
        /// The complete sequence.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        /// <returns>
        /// The <see cref="Chain"/>.
        /// </returns>
        private Chain GetSimpleSubsequence(Sequence sourceSequence, Subsequence subsequence)
        {
            ISequence bioSequence = sourceSequence.GetSubSequence(subsequence.Start, subsequence.Length);

            if (subsequence.SequenceAttribute.Any(sa => sa.Attribute == Attribute.Complement))
            {
                bioSequence = bioSequence.GetReverseComplementedSequence();
            }

            return new Chain(bioSequence.ConvertToString());
        }

        /// <summary>
        /// Extracts joined subsequence.
        /// </summary>
        /// <param name="sourceSequence">
        /// The complete sequence.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        /// <returns>
        /// The <see cref="Chain"/>.
        /// </returns>
        private Chain GetJoinedSubsequence(Sequence sourceSequence, Subsequence subsequence)
        {
            if (subsequence.SequenceAttribute.Any(sa => sa.Attribute == Attribute.Complement))
            {
                return GetJoinedSubsequenceWithComplement(sourceSequence, subsequence);
            }
            else
            {
                return GetJoinedSubsequenceWithoutComplement(sourceSequence, subsequence);
            }
        }

        /// <summary>
        /// Extracts joined subsequence without complement flag.
        /// </summary>
        /// <param name="sourceSequence">
        /// The complete sequence.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        /// <returns>
        /// The <see cref="Chain"/>.
        /// </returns>
        private Chain GetJoinedSubsequenceWithoutComplement(Sequence sourceSequence, Subsequence subsequence)
        {
            string joinedSequence = sourceSequence.GetSubSequence(subsequence.Start, subsequence.Length).ConvertToString();

            Position[] positions = subsequence.Position.ToArray();

            foreach (Position position in positions)
            {
                joinedSequence += sourceSequence.GetSubSequence(position.Start, position.Length).ConvertToString();
            }

            return new Chain(joinedSequence);
        }

        /// <summary>
        /// Extracts joined subsequence with complement flag.
        /// </summary>
        /// <param name="sourceSequence">
        /// The complete sequence.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        /// <returns>
        /// The <see cref="Chain"/>.
        /// </returns>
        private Chain GetJoinedSubsequenceWithComplement(Sequence sourceSequence, Subsequence subsequence)
        {
            ISequence bioSequence = sourceSequence.GetSubSequence(subsequence.Start, subsequence.Length);
            Position[] positions = subsequence.Position.ToArray();
            string resultSequence;

            if (subsequence.SequenceAttribute.Any(sa => sa.Attribute == Attribute.ComplementJoin))
            {
                string joinedSequence = bioSequence.ConvertToString();

                foreach (Position position in positions)
                {
                    joinedSequence += sourceSequence.GetSubSequence(position.Start, position.Length).ConvertToString();
                }

                resultSequence = new Sequence(Alphabets.DNA, joinedSequence).GetReverseComplementedSequence().ConvertToString();
            }
            else
            {
                resultSequence = bioSequence.GetReverseComplementedSequence().ConvertToString();

                foreach (Position position in positions)
                {
                    resultSequence += sourceSequence.GetSubSequence(position.Start, position.Length).GetReverseComplementedSequence().ConvertToString();
                }
            }

            return new Chain(resultSequence);
        }
    }
}
