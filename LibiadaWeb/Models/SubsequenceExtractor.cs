namespace LibiadaWeb.Models
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;

    using LibiadaWeb.Models.Repositories.Sequences;

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
        /// The extract chains.
        /// </summary>
        /// <param name="subsequences">
        /// The subsequences.
        /// </param>
        /// <param name="chainId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Chain}"/>.
        /// </returns>
        public Chain[] ExtractChains(Subsequence[] subsequences)
        {
            var parentChain = commonSequenceRepository.ToLibiadaBaseChain(subsequences[0].SequenceId).ToString();
            var sourceSequence = new Sequence(Alphabets.DNA, parentChain);
            var result = new Chain[subsequences.Length];

            for (int i = 0; i < subsequences.Length; i++)
            {
                result[i] = subsequences[i].Position.Count == 0
                        ? ExtractSimpleSubsequence(sourceSequence, subsequences[i])
                        : ExtractJoinedSubsequence(sourceSequence, subsequences[i]);
            }

            return result;
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
        public Subsequence[] GetSubsequences(long sequenceId, IEnumerable<Feature> features)
        {
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
        public Subsequence[] GetSubsequences(long sequenceId, IEnumerable<Feature> features, string[] filters)
        {
            var allSubsequences = db.Subsequence.Where(s => s.SequenceId == sequenceId && features.Contains(s.Feature))
                                        .Include(s => s.Position)
                                        .Include(s => s.SequenceAttribute)
                                        .ToArray();
            var tempResult = new List<Subsequence>();
            for (int i = 0; i < allSubsequences.Length; i++)
            {
                for (int j = 0; j < filters.Length; j++)
                {
                    if (allSubsequences[i].SequenceAttribute.Any(sa => sa.Attribute == Attribute.Product))
                    {
                        string value = allSubsequences[i].SequenceAttribute.Single(sa => sa.Attribute == Attribute.Product).Value;
                        if (value.Contains(filters[j]))
                        {
                            tempResult.Add(allSubsequences[i]);
                            break;
                        }
                    }
                }
            }

            return tempResult.ToArray();
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
        private Chain ExtractSimpleSubsequence(Sequence sourceSequence, Subsequence subsequence)
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
        private Chain ExtractJoinedSubsequence(Sequence sourceSequence, Subsequence subsequence)
        {
            if (subsequence.SequenceAttribute.Any(sa => sa.Attribute == Attribute.Complement))
            {
                return ExtractJoinedSubsequenceWithComplement(sourceSequence, subsequence);
            }
            else
            {
                return ExtractJoinedSubsequenceWithoutComplement(sourceSequence, subsequence);
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
        private Chain ExtractJoinedSubsequenceWithoutComplement(Sequence sourceSequence, Subsequence subsequence)
        {
            var joinedSequence = sourceSequence.GetSubSequence(subsequence.Start, subsequence.Length).ConvertToString();

            var position = subsequence.Position.ToArray();

            for (int j = 0; j < position.Length; j++)
            {
                joinedSequence += sourceSequence.GetSubSequence(position[j].Start, position[j].Length).ConvertToString();
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
        private Chain ExtractJoinedSubsequenceWithComplement(Sequence sourceSequence, Subsequence subsequence)
        {
            var bioSequence = sourceSequence.GetSubSequence(subsequence.Start, subsequence.Length);
            var position = subsequence.Position.ToArray();
            string resultSequence;

            if (subsequence.SequenceAttribute.Any(sa => sa.Attribute == Attribute.ComplementJoin))
            {
                var joinedSequence = bioSequence.ConvertToString();

                for (int j = 0; j < position.Length; j++)
                {
                    joinedSequence += sourceSequence.GetSubSequence(position[j].Start, position[j].Length).ConvertToString();
                }

                resultSequence = new Sequence(Alphabets.DNA, joinedSequence).GetReverseComplementedSequence().ConvertToString();
            }
            else
            {
                resultSequence = bioSequence.GetReverseComplementedSequence().ConvertToString();

                for (int j = 0; j < position.Length; j++)
                {
                    resultSequence += sourceSequence.GetSubSequence(position[j].Start, position[j].Length).GetReverseComplementedSequence().ConvertToString();
                }
            }

            return new Chain(resultSequence);
        }
    }
}
