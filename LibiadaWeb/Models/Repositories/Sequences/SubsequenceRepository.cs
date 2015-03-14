namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Misc.Iterators;

    /// <summary>
    /// The gene repository.
    /// </summary>
    public class SubsequenceRepository : ISubsequenceRepository
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
        /// Initializes a new instance of the <see cref="SubsequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public SubsequenceRepository(LibiadaWebEntities db)
        {
            this.db = db;
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// The extract chains.
        /// </summary>
        /// <param name="pieces">
        /// The pieces.
        /// </param>
        /// <param name="chainId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Chain}"/>.
        /// </returns>
        public List<Chain> ConvertToChains(List<Position> pieces, long chainId)
        {
            var starts = pieces.Select(p => p.Start).ToList();

            var stops = pieces.Select(p => p.Start + p.Length).ToList();

            BaseChain parentChain = commonSequenceRepository.ToLibiadaBaseChain(chainId);

            var iterator = new DefaultCutRule(starts, stops);

            var stringChains = DiffCutter.Cut(parentChain.ToString(), iterator);

            var chains = new List<Chain>();

            for (int i = 0; i < stringChains.Count; i++)
            {
                chains.Add(new Chain(stringChains[i]));
            }

            return chains;
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
        /// <param name="subsequences">
        /// The genes.
        /// </param>
        /// <returns>
        /// The <see cref="List{Chain}"/>.
        /// </returns>
        public List<Chain> ExtractSequences(long sequenceId, int[] featureIds, out List<Subsequence> subsequences)
        {
            subsequences = db.Subsequence.Where(g => g.SequenceId == sequenceId && featureIds.Contains(g.FeatureId)).Include(g => g.Position).Include(g => g.SequenceAttribute).ToList();

            var pieces = subsequences.Select(g => g.Position.First()).ToList();

            var sequences = ConvertToChains(pieces, sequenceId);

            return sequences;
        }

        /// <summary>
        /// The extract sequences.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <param name="subsequences">
        /// The genes.
        /// </param>
        /// <returns>
        /// The <see cref="List{Chain}"/>.
        /// </returns>
        public List<Chain> ExtractSequences(long matterId, int notationId, int[] featureIds, out List<Subsequence> subsequences)
        {
            var sequenceId = db.DnaSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
            return ExtractSequences(sequenceId, featureIds, out subsequences);
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
