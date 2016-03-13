namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Bio.IO.GenBank;
    using Bio.Util;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The subsequence repository.
    /// </summary>
    public class SubsequenceRepository : ISubsequenceRepository
    {
        /// <summary>
        /// The database context.
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
        /// The database context.
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

            CreateFeatureSubsequences(features, sequenceId);
        }

        /// <summary>
        /// The update annotations.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        public void UpdateAnnotations(List<FeatureItem> features, long sequenceId)
        {
            var localSubsequences = db.Subsequence
                    .Include(s => s.Feature)
                    .Include(s => s.Position)
                    .Include(s => s.SequenceAttribute)
                    .Where(s => s.SequenceId == sequenceId && s.FeatureId != Aliases.Feature.NonCodingSequence).ToList();

            for (int i = 1; i < features.Count; i++)
            {
                UpdateAnnotation(features[i], localSubsequences);
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

            var localSubsequences = db.Subsequence
                    .Include(s => s.Feature)
                    .Include(s => s.Position)
                    .Include(s => s.SequenceAttribute)
                    .Where(s => s.SequenceId == sequenceId && s.FeatureId != Aliases.Feature.NonCodingSequence).ToList();
            
            var missingRemoteFeatures = new List<FeatureItem>();

            for (int i = 1; i < features.Count; i++)
            {
                var missingFeature = CheckFeature(features[i], localSubsequences);
                if (missingFeature != null)
                {
                    missingRemoteFeatures.Add(missingFeature);
                }
            }

            var annotationsUpdatable = missingRemoteFeatures.Count == 0 && localSubsequences.Count > 0;

            return new Dictionary<string, object>
                       {
                           { "attributes", attributeRepository.Attributes },
                           { "localSubsequences", localSubsequences },
                           { "missingRemoteFeatures", missingRemoteFeatures },
                           { "annotationsUpdatable", annotationsUpdatable }
                       };
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
        private void CheckImportability(List<FeatureItem> features, long sequenceId)
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
                    bool pseudo = feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudo)) ||
                                  feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudogene));
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

                if (leafLocations.Any(leafLocation => leafLocation.LocationEnd < leafLocation.LocationStart))
                {
                    throw new Exception("Subsequence length cant be less than 1.");
                }
            }
        }

        /// <summary>
        /// The check feature.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        /// <param name="localSubsequences">
        /// The local subsequences.
        /// </param>
        /// <returns>
        /// The missing features <see cref="FeatureItem"/>.
        /// </returns>
        private FeatureItem CheckFeature(FeatureItem feature, List<Subsequence> localSubsequences)
        {
            int featureId;

            if (feature.Key == "gene")
            {
                bool pseudo = feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudo)) || 
                              feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudogene));
                if (!pseudo)
                {
                    return null;
                }

                featureId = Aliases.Feature.PseudoGen;
            }
            else
            {
                featureId = featureRepository.GetFeatureIdByName(feature.Key);
            }

            var location = feature.Location;
            var leafLocations = location.GetLeafLocations();
            var partial = CheckPartial(leafLocations);
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

            var equalLocalSubsequences = localSubsequences.Where(s => s.Start == start 
                                                                   && s.Length == length 
                                                                   && s.FeatureId == featureId 
                                                                   && s.Partial == partial).ToList();

            if (equalLocalSubsequences.Any())
            {
                var localSubsequence = equalLocalSubsequences.Single();
                var localPositions = localSubsequence.Position.ToList();
                
                sequenceAttributeRepository.CheckSequenceAttributes(feature, localSubsequence, ref complement, ref complementJoin, ref partial);

                for (int k = 1; k < leafLocations.Count; k++)
                {
                    var leafLocation = leafLocations[k];
                    var leafStart = leafLocation.LocationStart - 1;
                    var leafEnd = leafLocation.LocationEnd - 1;
                    var leafLength = leafEnd - leafStart + 1;

                    if (localPositions.Count(p => p.Start == leafStart && p.Length == leafLength) != 1)
                    {
                        return feature;
                    }
                }

                if (localSubsequence.SequenceAttribute.Count == 0 && !complement && !complementJoin && !partial)
                {
                    localSubsequences.Remove(localSubsequence);
                }

                return null;
            }
            else
            {
                return feature;
            }
        }

        /// <summary>
        /// The update annotation.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        /// <param name="localSubsequences">
        /// The local subsequences.
        /// </param>
        private void UpdateAnnotation(FeatureItem feature, List<Subsequence> localSubsequences)
        {
            throw new NotImplementedException();
            int featureId;

            if (feature.Key == "gene")
            {
                bool pseudo = feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudo)) ||
                              feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudogene));
                if (!pseudo)
                {
                    throw new ArgumentException("No genes allowed here", "feature");
                }

                featureId = Aliases.Feature.PseudoGen;
            }
            else
            {
                featureId = featureRepository.GetFeatureIdByName(feature.Key);
            }

            var location = feature.Location;
            var leafLocations = location.GetLeafLocations();
            var partial = CheckPartial(leafLocations);
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

            var equalLocalSubsequences = localSubsequences.Where(s => s.Start == start
                                                                   && s.Length == length
                                                                   && s.FeatureId == featureId
                                                                   && s.Partial == partial).ToList();

            if (equalLocalSubsequences.Any())
            {
                var localSubsequence = equalLocalSubsequences.Single();
                var localPositions = localSubsequence.Position.ToList();

                for (int k = 1; k < leafLocations.Count; k++)
                {
                    var leafLocation = leafLocations[k];
                    var leafStart = leafLocation.LocationStart - 1;
                    var leafEnd = leafLocation.LocationEnd - 1;
                    var leafLength = leafEnd - leafStart + 1;

                    if (localPositions.Count(p => p.Start == leafStart && p.Length == leafLength) != 1)
                    {
                    }
                }

                if (localSubsequence.SequenceAttribute.Count == 0 && !complement && !complementJoin && !partial)
                {
                    localSubsequences.Remove(localSubsequence);
                }
            }
        }

        /// <summary>
        /// Create subsequences from features
        /// and noncoding subsequences from gaps.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        private void CreateFeatureSubsequences(List<FeatureItem> features, long sequenceId)
        {
            var newSubsequences = new List<Subsequence>();
            var newPositions = new List<Position>();
            var newSequenceAttributes = new List<SequenceAttribute>();
            var positionsMap = new bool[features[0].Location.LocationEnd];

            for (int i = 1; i < features.Count; i++)
            {
                var feature = features[i];
                var location = feature.Location;
                var leafLocations = location.GetLeafLocations();
                int featureId;

                if (feature.Key == "source")
                {
                    throw new Exception("Sequence seems to be chimeric as it is several 'source' records in file. Second source location = " + leafLocations[0].StartData);
                }

                if (feature.Key == "gene")
                {
                    bool pseudo = feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudo)) ||
                                  feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudogene));
                    if (!pseudo)
                    {
                        var nextFeature = features[i + 1];
                        var nextLocation = nextFeature.Location;
                        var nextLeafLocations = nextLocation.GetLeafLocations();
                        
                        if (nextLeafLocations.Count != leafLocations.Count)
                        {
                            if (nextLeafLocations.Count > leafLocations.Count && 
                                leafLocations[0].LocationStart == nextLeafLocations[0].LocationStart && 
                                leafLocations[0].LocationEnd == nextLeafLocations[nextLeafLocations.Count - 1].LocationEnd)
                            {
                                continue;
                            }

                            throw new Exception("Gene and next element's locations are not equal. Location = " + leafLocations[0].StartData);
                        }

                        for (int j = 0; j < leafLocations.Count; j++)
                        {
                            if (leafLocations[j].LocationStart != nextLeafLocations[j].LocationStart ||
                                leafLocations[j].LocationEnd != nextLeafLocations[j].LocationEnd ||
                                leafLocations[j].StartData != nextLeafLocations[j].StartData ||
                                leafLocations[j].EndData != nextLeafLocations[j].EndData)
                            {
                                throw new Exception("Gene and next element's locations are not equal. Location = " + leafLocations[j].StartData);
                            }
                        }

                        continue;
                    }

                    featureId = Aliases.Feature.PseudoGen;
                }
                else
                {
                    featureId = featureRepository.GetFeatureIdByName(feature.Key);
                }

                bool partial = CheckPartial(leafLocations);
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
                    Partial = partial,
                    SequenceId = sequenceId,
                    Start = start,
                    Length = length,
                    RemoteId = location.Accession
                };

                newSubsequences.Add(subsequence);

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

                    newPositions.Add(position);

                    AddPositionToMap(positionsMap, leafStart, leafEnd);
                }

                newSequenceAttributes.AddRange(sequenceAttributeRepository.CreateSubsequenceAttributes(feature.Qualifiers, complement, complementJoin, subsequence));
            }

            newSubsequences.AddRange(CreateNonCodingSubsequences(positionsMap, sequenceId));

            db.Subsequence.AddRange(newSubsequences);
            db.Position.AddRange(newPositions);
            db.SequenceAttribute.AddRange(newSequenceAttributes);

            db.SaveChanges();
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
        /// <returns>
        /// The <see cref="List{Subsequence}"/>.
        /// </returns>
        private List<Subsequence> CreateNonCodingSubsequences(bool[] positionsMap, long sequenceId)
        {
            var positions = ExtractNonCodingSubsequences(positionsMap);
            var starts = positions[0];
            var lengths = positions[1];
            var result = new List<Subsequence>();

            for (int i = 0; i < lengths.Count; i++)
            {
                var subsequence = new Subsequence
                {
                    Id = DbHelper.GetNewElementId(db),
                    FeatureId = Aliases.Feature.NonCodingSequence,
                    Partial = false,
                    SequenceId = sequenceId,
                    Start = starts[i],
                    Length = lengths[i]
                };

                result.Add(subsequence);
            }

            return result;
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

        /// <summary>
        /// The check partial.
        /// </summary>
        /// <param name="leafLocations">
        /// The leaf locations.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CheckPartial(List<ILocation> leafLocations)
        {
            return leafLocations.Any(leafLocation => leafLocation.LocationStart.ToString() != leafLocation.StartData 
                                                  || leafLocation.LocationEnd.ToString() != leafLocation.EndData);
        }
    }
}
