namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Bio.IO.GenBank;

    using LibiadaCore.Core;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;

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
        /// The feature repository.
        /// </summary>
        private readonly FeatureRepository featureRepository;

        /// <summary>
        /// The attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

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
            featureRepository = new FeatureRepository(db);
            sequenceAttributeRepository = new SequenceAttributeRepository(db);
        }

        /// <summary>
        /// The create feature subsequences.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <param name="starts">
        /// The starts.
        /// </param>
        /// <param name="ends">
        /// The ends.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if feature contains no leaf location or  
        /// if features positions order is not ascending or 
        /// if feature length is less than 1.
        /// </exception>
        public void CreateFeatureSubsequences(List<FeatureItem> features, long sequenceId, out List<int> starts, out List<int> ends)
        {
            starts = new List<int>();
            ends = new List<int> { 0 };

            for (int i = 1; i < features.Count; i++)
            {
                var feature = features[i];
                int featureId;

                if (feature.Key == "gene")
                {
                    bool pseudo = feature.Qualifiers.Any(qualifier => qualifier.Key == "pseudo");
                    if (!pseudo)
                    {
                        continue;
                    }

                    featureId = Aliases.Feature.PseudoGen;
                }
                else
                {
                    featureId = featureRepository.GetFeatureIdByName(feature.Key);
                }

                var location = feature.Location;
                var leafLocations = feature.Location.GetLeafLocations();

                if (leafLocations.Count == 0)
                {
                    throw new Exception("No leaf locations");
                }

                bool complement = location.Operator == LocationOperator.Complement;
                bool join = leafLocations.Count > 1;
                bool complementJoin = join && complement;

                complement = complement || location.SubLocations[0].Operator == LocationOperator.Complement;

                int start = leafLocations[0].LocationStart - 1;
                int end = leafLocations[0].LocationEnd - 1;
                int length = end - start + 1;

                if (length < 1)
                {
                    throw new Exception("Length of subsequence cant be less than 1.");
                }

                if (ends[i] < ends[i - 1])
                {
                    throw new Exception("Wrong subsequences order.");
                }

                var subsequence = new Subsequence
                {
                    Id = DbHelper.GetNewElementId(db),
                    FeatureId = featureId,
                    Partial = false,
                    Complementary = complement,
                    SequenceId = sequenceId,
                    Start = start,
                    Length = end - start + 1
                };

                db.Subsequence.Add(subsequence);

                starts.Add(start - 1);
                ends.Add(end + 1);

                for (int k = 1; k > leafLocations.Count; k++)
                {
                    var leafLocation = leafLocations[k];

                    start = leafLocation.LocationStart - 1;
                    end = leafLocation.LocationEnd - 1;

                    var position = new Position
                    {
                        Subsequence = subsequence,
                        Start = start,
                        Length = end - start + 1
                    };

                    db.Position.Add(position);

                    starts.Add(start - 1);
                    ends.Add(end + 1);
                }

                sequenceAttributeRepository.CreateSequenceAttributes(feature.Qualifiers, complement, complementJoin,  subsequence);
            }

            starts.Add(features[0].Location.LocationEnd - 1);
        }

        /// <summary>
        /// Creates non coding subsequences.
        /// </summary>
        /// <param name="starts">
        /// The starts.
        /// </param>
        /// <param name="ends">
        /// The ends.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        public void CreateNonCodingSubsequences(List<int> starts, List<int> ends, long sequenceId)
        {
            for (int i = 0; i < ends.Count; i++)
            {
                int start = ends[i];
                int length = starts[i] - ends[i] + 1;

                if (length > 0)
                {
                    var subsequence = new Subsequence
                    {
                        Id = DbHelper.GetNewElementId(db),
                        FeatureId = Aliases.Feature.NonCodingSequence,
                        Partial = false,
                        Complementary = false,
                        SequenceId = sequenceId,
                        Start = start,
                        Length = length
                    };

                    db.Subsequence.Add(subsequence);
                }
            }
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
