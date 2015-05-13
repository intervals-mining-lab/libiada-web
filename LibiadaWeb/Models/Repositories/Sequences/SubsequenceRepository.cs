namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Bio.IO.GenBank;

    using LibiadaWeb.Helpers;
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

        /// <summary>
        /// The common sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The attribute repository.
        /// </summary>
        private readonly AttributeRepository attributeRepository;

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
        /// Adds all subsequences of given sequence to database.
        /// </summary>
        /// <param name="features">
        /// The subsequences.
        /// </param>
        /// <param name="sequenceId">
        /// The source sequence id.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if error occurs during importability check.
        /// </exception>
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


            var positionsMap = CreateFeatureSubsequences(features, sequenceId);
            CreateNonCodingSubsequences(positionsMap, sequenceId);

            db.SaveChanges();
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
        /// if source length not equals to parent sequence length or 
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
                    bool pseudo = feature.Qualifiers.Any(qualifier => qualifier.Key == attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudo));
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
                    int start = leafLocation.LocationStart - 1;
                    int end = leafLocation.LocationEnd - 1;
                    int length = end - start + 1;

                    if (length < 1)
                    {
                        throw new Exception("Subsequence length cant be less than 1.");
                    }
                }
            }
        }

        /// <summary>
        /// The check annotations.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if error occurs during importability check.
        /// </exception>
        public Dictionary<string, object> CheckAnnotations(List<FeatureItem> features, long sequenceId)
        {
            try
            {
                CheckImportability(features, sequenceId);
            }
            catch (Exception e)
            {
                throw new Exception("Error occured during importability check.", e);
            }

            var localSubsequences = db.Subsequence.Include(s => s.Feature)
                    .Include(s => s.Position)
                    .Include(s => s.SequenceAttribute)
                    .Where(s => s.SequenceId == sequenceId && s.FeatureId != Aliases.Feature.NonCodingSequence).ToList();

            var missingRemoteFeatures = new List<FeatureItem>();

            for (int i = 1; i < features.Count; i++)
            {
                var feature = features[i];
                int featureId;

                if (feature.Key == "gene")
                {
                    bool pseudo = feature.Qualifiers.Any(qualifier => qualifier.Key == attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudo));
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
                bool partial = false;
                bool complement = location.Operator == LocationOperator.Complement;
                bool join = leafLocations.Count > 1;
                bool complementJoin = join && complement;

                if (location.SubLocations.Count > 0)
                {
                    complement = complement || location.SubLocations[0].Operator == LocationOperator.Complement;
                }

                foreach (var leafLocation in leafLocations)
                {
                    if (leafLocation.LocationStart.ToString() != leafLocation.StartData
                       || leafLocation.LocationEnd.ToString() != leafLocation.EndData)
                    {
                        partial = true;
                    }
                }

                int start = leafLocations[0].LocationStart - 1;
                int end = leafLocations[0].LocationEnd - 1;
                int length = end - start + 1;

                var equalLocalSubsequences = localSubsequences.Where(s => s.Start == start
                                                                 && s.Length == length
                                                                 && s.FeatureId == featureId
                                                                 && s.Partial == partial
                                                                 && s.Complementary == complement).ToList();

                if (equalLocalSubsequences.Any())
                {
                    var localSubsequence = equalLocalSubsequences.First();
                    var localPositions = localSubsequence.Position.ToList();
                    var localAttributes = localSubsequence.SequenceAttribute.ToList();

                    // TODO: attributes check

                    for (int k = 1; k < leafLocations.Count; k++)
                    {
                        var leafLocation = leafLocations[k];
                        var leafStart = leafLocation.LocationStart - 1;
                        var leafEnd = leafLocation.LocationEnd - 1;
                        var leafLength = leafEnd - leafStart + 1;

                        if (localPositions.Count(p => p.Start == leafStart && p.Length == leafLength) != 1)
                        {
                            missingRemoteFeatures.Add(feature);
                        }
                    }

                    localSubsequences.Remove(localSubsequence);
                }
                else
                {
                    missingRemoteFeatures.Add(feature);
                }
            }

            return new Dictionary<string, object>
                       {
                           { "localSubsequences", localSubsequences },
                           { "missingRemoteFeatures", missingRemoteFeatures }
                       };
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
        /// The <see cref="T:bool[]"/>.
        /// </returns>
        private bool[] CreateFeatureSubsequences(List<FeatureItem> features, long sequenceId)
        {
            var positionsMap = new bool[features[0].Location.LocationEnd];

            for (int i = 1; i < features.Count; i++)
            {
                var feature = features[i];
                int featureId;

                if (feature.Key == "gene")
                {
                    bool pseudo = feature.Qualifiers.Any(qualifier => qualifier.Key == attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudo));
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
                bool partial = false;
                bool complement = location.Operator == LocationOperator.Complement;
                bool join = leafLocations.Count > 1;
                bool complementJoin = join && complement;

                if (location.SubLocations.Count > 0)
                {
                    complement = complement || location.SubLocations[0].Operator == LocationOperator.Complement;
                }

                foreach (var leafLocation in leafLocations)
                {
                    if (leafLocation.LocationStart.ToString() != leafLocation.StartData || leafLocation.LocationEnd.ToString() != leafLocation.EndData)
                    {
                        partial = true;
                    }
                }

                int start = leafLocations[0].LocationStart - 1;
                int end = leafLocations[0].LocationEnd - 1;
                int length = end - start + 1;

                var subsequence = new Subsequence
                {
                    Id = DbHelper.GetNewElementId(db),
                    FeatureId = featureId,
                    Partial = partial,
                    Complementary = complement,
                    SequenceId = sequenceId,
                    Start = start,
                    Length = length
                };

                db.Subsequence.Add(subsequence);

                AddPositionToMap(positionsMap, start, end);

                for (int k = 1; k < leafLocations.Count; k++)
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

                    AddPositionToMap(positionsMap, leafStart, leafEnd);
                }

                sequenceAttributeRepository.CreateSubsequenceAttributes(feature.Qualifiers, complement, complementJoin, subsequence);
            }

            return positionsMap;
        }

        /// <summary>
        /// Creates non coding subsequences.
        /// </summary>
        /// <param name="positionsMap">
        /// The positions map.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        private void CreateNonCodingSubsequences(bool[] positionsMap, long sequenceId)
        {
            var positions = ExtractNonCodingSubsequences(positionsMap);
            var starts = positions[0];
            var lengths = positions[1];

            for (int i = 0; i < lengths.Count; i++)
            {
                var subsequence = new Subsequence
                {
                    Id = DbHelper.GetNewElementId(db),
                    FeatureId = Aliases.Feature.NonCodingSequence,
                    Partial = false,
                    Complementary = false,
                    SequenceId = sequenceId,
                    Start = starts[i],
                    Length = lengths[i]
                };

                db.Subsequence.Add(subsequence);
            }
        }

        /// <summary>
        /// The add position to map.
        /// </summary>
        /// <param name="positionsMap">
        /// The positions map.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        private void AddPositionToMap(bool[] positionsMap, int start, int end)
        {
            for (int j = start; j <= end; j++)
            {
                positionsMap[j] = true;
            }
        }

        /// <summary>
        /// The extract non coding subsequences.
        /// </summary>
        /// <param name="positionsMap">
        /// The positions map.
        /// </param>
        /// <returns>
        /// The <see cref="T:List{Int32}[]"/>.
        /// </returns>
        private List<int>[] ExtractNonCodingSubsequences(bool[] positionsMap)
        {
            var starts = new List<int>();
            var lengths = new List<int>();
            var currentStart = 0;
            var currentLength = 0;

            for (int i = 0; i < positionsMap.Length; i++)
            {
                if (positionsMap[i])
                {
                    if (currentLength > 0)
                    {
                        starts.Add(currentStart);
                        lengths.Add(currentLength);
                    }

                    currentStart = i + 1;
                    currentLength = 0;
                }
                else
                {
                    currentLength++;
                }
            }

            if (currentLength > 0)
            {
                starts.Add(currentStart);
                lengths.Add(currentLength);
            }

            return new[] { starts, lengths };
        }
    }
}
