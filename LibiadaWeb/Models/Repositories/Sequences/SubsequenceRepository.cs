namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Bio.IO.GenBank;

    using LibiadaCore.Misc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The subsequence repository.
    /// </summary>
    public class SubsequenceRepository : ISubsequenceRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The feature repository.
        /// </summary>
        private readonly FeatureRepository featureRepository;

        /// <summary>
        /// The attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

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
            featureRepository = new FeatureRepository(db);
            sequenceAttributeRepository = new SequenceAttributeRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// Checks importability of subsequences.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if subsequences are not importable.
        /// Thrown if feature contains no leaf location or  
        /// if features positions order is not ascending or 
        /// if feature length is less than 1.
        /// </exception>
        public void CheckImportability(List<FeatureItem> features, long sequenceId)
        {
            var parentSequence = commonSequenceRepository.ToLibiadaBaseChain(sequenceId);

            var parentLength = parentSequence.GetLength();
            var sourceLength = features[0].Location.LocationEnd;

            if (parentLength != sourceLength)
            {
                throw new Exception("Parent and source lengthes are not equal. Parent length = " + parentLength 
                                                                           + " source length = " + sourceLength);
            }

            for (int i = 1; i < features.Count; i++)
            {
                var feature = features[i];

                if (feature.Key == "gene")
                {
                    bool pseudo = feature.Qualifiers.Any(qualifier => qualifier.Key == "pseudo");
                    if (!pseudo)
                    {
                        continue;
                    }
                }
                else
                {
                    if (!featureRepository.FeatureExists(feature.Key))
                    {
                        throw new Exception("Unknown feature. Feature name = " + feature.Key);
                    }
                }

                var location = feature.Location;

                if (location.SubLocations.Count > 0)
                {
                    var subLocationOperator = location.SubLocations[0].Operator;

                    foreach (var subLocation in location.SubLocations)
                    {
                        if (subLocation.Operator != subLocationOperator)
                        {
                            throw new Exception("SubLocation operators does not match: " + subLocationOperator 
                                                                               + " and " + subLocation.Operator);
                        }
                    }
                }

                var leafLocations = feature.Location.GetLeafLocations();

                if (leafLocations.Count == 0)
                {
                    throw new Exception("No leaf locations");
                }

                foreach (var leafLocation in leafLocations)
                {
                    if (leafLocation.LocationStart.ToString() != leafLocation.StartData)
                    {
                        throw new Exception("Location and location data are not equal: location start = " + leafLocation.LocationStart 
                                                                                   + " start data = " + leafLocation.StartData);
                    }

                    if (leafLocation.LocationEnd.ToString() != leafLocation.EndData)
                    {
                        throw new Exception("Location and location data are not equal: location end = " + leafLocation.LocationEnd
                                                                                   + " end data = " + leafLocation.EndData);
                    }

                    int start = leafLocation.LocationStart - 1;
                    int end = leafLocation.LocationEnd - 1;
                    int length = end - start + 1;

                    if (length < 1)
                    {
                        throw new Exception("Length of subsequence cant be less than 1.");
                    }
                }

                foreach (var qualifier in feature.Qualifiers)
                {
                    if (qualifier.Key == "codon_start" && qualifier.Value[0] != "1")
                    {
                        throw new Exception("Codon start is not 1. value = " + qualifier.Value[0]);
                    }
                }
            }
        }

        /// <summary>
        /// Create subsequences from features.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="T:List{Int32}[]"/>.
        /// </returns>
        public List<int>[] CreateFeatureSubsequences(List<FeatureItem> features, long sequenceId)
        {
            var positions = new List<IntPair>();

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
                bool complement = location.Operator == LocationOperator.Complement;
                bool join = leafLocations.Count > 1;
                bool complementJoin = join && complement;
                if (location.SubLocations.Count > 0)
                {
                    complement = complement || location.SubLocations[0].Operator == LocationOperator.Complement;
                }

                int start = leafLocations[0].LocationStart - 1;
                int end = leafLocations[0].LocationEnd - 1;
                int length = end - start + 1;

                var subsequence = new Subsequence
                {
                    Id = DbHelper.GetNewElementId(db),
                    FeatureId = featureId,
                    Partial = false,
                    Complementary = complement,
                    SequenceId = sequenceId,
                    Start = start,
                    Length = length
                };

                db.Subsequence.Add(subsequence);

                positions.Add(new IntPair(start - 1, end + 1));

                for (int k = 1; k > leafLocations.Count; k++)
                {
                    var leafLocation = leafLocations[k];
                    var leafStart = leafLocation.LocationStart - 1;
                    var leafEnd = leafLocation.LocationEnd - 1;
                    var leafLength = leafEnd - leafStart + 1;

                    var position = new Position
                    {
                        SubsequenceId = subsequence.Id,
                        Start = leafStart,
                        Length = leafLength
                    };

                    db.Position.Add(position);

                    positions.Add(new IntPair(leafStart - 1, leafEnd + 1));
                }

                sequenceAttributeRepository.CreateSequenceAttributes(feature.Qualifiers, complement, complementJoin, subsequence);
            }

            var parentSequenceEnd = features[0].Location.LocationEnd - 1;

            return ProcessPositions(positions, parentSequenceEnd);
        }

        /// <summary>
        /// Creates non coding subsequences.
        /// </summary>
        /// <param name="starts">
        /// The subsequences starts.
        /// </param>
        /// <param name="ends">
        /// The subsequences ends.
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
        /// Adds all subsequences of given sequence to database.
        /// </summary>
        /// <param name="features">
        /// The subsequences.
        /// </param>
        /// <param name="sequenceId">
        /// The source sequence id.
        /// </param>
        public void CreateSubsequences(List<FeatureItem> features, long sequenceId)
        {
            try
            {
                CheckImportability(features, sequenceId);
            }
            catch (Exception e)
            {
                throw new Exception("Error occured during importability check.", e);
            }
            

            var positions = CreateFeatureSubsequences(features, sequenceId);
            CreateNonCodingSubsequences(positions[0], positions[1], sequenceId);

            db.SaveChanges();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// The process positions.
        /// </summary>
        /// <param name="positions">
        /// The positions.
        /// </param>
        /// <param name="parentSequenceEnd">
        /// The parent sequence length.
        /// </param>
        /// <returns>
        /// The <see cref="T:List{Int32}[]"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if starts or ends are not sorted.
        /// </exception>
        private List<int>[] ProcessPositions(List<IntPair> positions, int parentSequenceEnd)
        {
            positions = positions.OrderBy(p => p.First).ToList();

            var starts = positions.Select(p => p.First).ToList();
            var ends = positions.Select(p => p.Second).ToList();

            starts.Add(parentSequenceEnd);
            ends.Insert(0, 0);

            if (!ArrayManipulator.IsSorted(starts) || !ArrayManipulator.IsSorted(ends))
            {
                throw new Exception("Wrong subsequences order.");
            }

            return new[] { starts, ends };
        }
    }
}
