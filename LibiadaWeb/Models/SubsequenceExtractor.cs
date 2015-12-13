namespace LibiadaWeb.Models
{
    using System;
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
        public List<Chain> ExtractChains(List<Subsequence> subsequences, long chainId)
        {
            var parentChain = commonSequenceRepository.ToLibiadaBaseChain(chainId).ToString();
            var sourceSequence = new Sequence(Alphabets.DNA, parentChain);
            var result = new List<Chain>();

            foreach (Subsequence subsequence in subsequences)
            {
                result.Add(subsequence.Position.Count == 0
                        ? ExtractSimpleSubsequence(sourceSequence, subsequence)
                        : ExtractJoinedSubsequence(sourceSequence, subsequence));
            }

            return result;
        }

        /// <summary>
        /// The extract sequences.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="List{Subsequence}"/>.
        /// </returns>
        public List<Subsequence> GetSubsequences(long sequenceId, int[] featureIds)
        {
            return db.Subsequence.Where(g => g.SequenceId == sequenceId && featureIds.Contains(g.FeatureId))
                                        .Include(g => g.Position)
                                        .Include(g => g.SequenceAttribute).ToList();
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

            if (subsequence.SequenceAttribute.Any(sa => sa.AttributeId == Aliases.Attribute.Complement))
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
            if (subsequence.SequenceAttribute.Any(sa => sa.AttributeId == Aliases.Attribute.Complement))
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

            if (subsequence.SequenceAttribute.Any(sa => sa.AttributeId == Aliases.Attribute.ComplementJoin))
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
