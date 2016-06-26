namespace LibiadaWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Bio.IO.GenBank;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The subsequence importer.
    /// </summary>
    public class SubsequenceImporter : IDisposable
    {
        /// <summary>
        /// The database context.
        /// </summary>
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        /// <summary>
        /// The feature repository.
        /// </summary>
        private readonly FeatureRepository featureRepository;

        /// <summary>
        /// The attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

        /// <summary>
        /// The attribute repository.
        /// </summary>
        private readonly AttributeRepository attributeRepository;

        /// <summary>
        /// The sequence id.
        /// </summary>
        private readonly long sequenceId;

        /// <summary>
        /// The features.
        /// </summary>
        private readonly List<FeatureItem> features;

        /// <summary>
        /// The parent length.
        /// </summary>
        private readonly int parentLength;

        /// <summary>
        /// The source length.
        /// </summary>
        private readonly int sourceLength;

        /// <summary>
        /// The all non genes leaf locations.
        /// </summary>
        private readonly ILocation[] allNonGenesLeafLocations;

        /// <summary>
        /// The positions map.
        /// </summary>
        private readonly bool[] positionsMap;

        /// <summary>
        /// Gene feature type name.
        /// </summary>
        private readonly string gene;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceImporter"/> class.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        public SubsequenceImporter(List<FeatureItem> features, long sequenceId)
        {
            this.features = features;
            this.sequenceId = sequenceId;
            featureRepository = new FeatureRepository(db);
            sequenceAttributeRepository = new SequenceAttributeRepository(db);
            attributeRepository = new AttributeRepository(db);

            using (var commonSequenceRepository = new CommonSequenceRepository(db))
            {
                var parentSequence = commonSequenceRepository.ToLibiadaBaseChain(sequenceId);
                parentLength = parentSequence.GetLength();
            }

            gene = featureRepository.GetFeatureById(Aliases.Feature.Gene).Type;
            sourceLength = features[0].Location.LocationEnd;
            positionsMap = new bool[parentLength];
            allNonGenesLeafLocations = features.Where(f => f.Key != gene)
                                               .Select(f => f.Location.GetLeafLocations())
                                               .Where(l => l.Count == 1)
                                               .Select(l => l[0])
                                               .ToArray();
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
        /// <exception cref="Exception">
        /// Thrown if error occurs during importability check.
        /// </exception>
        public void CreateSubsequences()
        {
            try
            {
                CheckImportability();
            }
            catch (Exception e)
            {
                throw new Exception("Error occured during importability check.", e);
            }

            CreateFeatureSubsequences();
        }

        /// <summary>
        /// Checks importability of subsequences.
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown if subsequences are not importable.
        /// Thrown if feature contains no leaf location or  
        /// if source length not equals to parent sequence length or 
        /// if feature length is less than 1.
        /// </exception>
        private void CheckImportability()
        {
            if (parentLength != sourceLength)
            {
                throw new Exception("Parent and source lengthes are not equal. Parent length = " + parentLength + " source length = " + sourceLength);
            }

            for (int i = 1; i < features.Count; i++)
            {
                var feature = features[i];
                var location = feature.Location;
                var leafLocations = location.GetLeafLocations();

                if (feature.Key == "source")
                {
                    throw new Exception("Sequence seems to be chimeric as it is several 'source' records in file. Second source location = " + leafLocations[0].StartData);
                }

                if (!featureRepository.FeatureExists(feature.Key))
                {
                    throw new Exception("Unknown feature. Feature name = " + feature.Key);
                }

                if (feature.Key == gene)
                {
                    if (!GeneImportable(leafLocations, i))
                    {
                        continue;
                    }
                }

                if (location.Operator == LocationOperator.Order || location.Operator == LocationOperator.Order)
                {
                    throw new Exception("Unknown operator: " + location.Operator);
                }

                if (location.SubLocations.Count > 0)
                {
                    var subLocationOperator = location.SubLocations[0].Operator;

                    if (subLocationOperator == LocationOperator.Order || subLocationOperator == LocationOperator.Order)
                    {
                        throw new Exception("Unknown operator: " + subLocationOperator);
                    }

                    foreach (var subLocation in location.SubLocations)
                    {
                        if (subLocation.Operator != subLocationOperator)
                        {
                            throw new Exception("SubLocation operators does not match: " + subLocationOperator + " and " + subLocation.Operator);
                        }
                    }
                }

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
        /// Checks if gene is valid and needs import.
        /// </summary>
        /// <param name="leafLocations">
        /// The leaf locations.
        /// </param>
        /// <param name="index">
        /// The index of gene.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool GeneImportable(List<ILocation> leafLocations, int index)
        {
            if (leafLocations.Count > 1)
            {
                // if feature is last
                if ((index + 1) >= features.Count)
                {
                    throw new Exception("Multipositional gene has no corresponding child feature. Location = " + leafLocations[0].StartData);
                }

                var nextLeafLocations = features[index + 1].Location.GetLeafLocations();
                if (nextLeafLocations.Count != leafLocations.Count)
                {
                    throw new Exception("Gene's and next element's locations are not equal. Location = " + leafLocations[0].StartData);
                }

                for (int j = 0; j < leafLocations.Count; j++)
                {
                    // comparing gene's positions and corresponding child feature's position
                    if (leafLocations[j].LocationStart != nextLeafLocations[j].LocationStart
                     || leafLocations[j].LocationEnd != nextLeafLocations[j].LocationEnd)
                    {
                        throw new Exception("Multipositional gene and corresponding child feature's locations are not equal. Location = " + leafLocations[j].StartData);
                    }
                }

                // don't need to import this gene
                return false;
            }

            var leafLocation = leafLocations[0];

            // if feature is not last
            if ((index + 1) < features.Count)
            {
                var nextLeafLocations = features[index + 1].Location.GetLeafLocations();

                // if there is join in child record parent record contains only
                // first child start and last child end
                if (nextLeafLocations.Count > 1)
                {
                    if (leafLocation.LocationStart == nextLeafLocations[0].LocationStart
                     && leafLocation.LocationEnd == nextLeafLocations[nextLeafLocations.Count - 1].LocationEnd)
                    {
                        // don't need to import this gene
                        return false;
                    }

                    throw new Exception("Gene's and next element's locations are not equal. Location = " + leafLocation.StartData);
                }
            }

            // checking if there is any feature with identical position
            if (allNonGenesLeafLocations.Any(l => leafLocation.LocationStart == l.LocationStart && leafLocation.LocationEnd == l.LocationEnd
                                               && leafLocation.StartData == l.StartData && leafLocation.EndData == l.EndData))
            {
                // don't need to import this gene
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create subsequences from features
        /// and noncoding subsequences from gaps.
        /// </summary>
        private void CreateFeatureSubsequences()
        {
            var newSubsequences = new List<Subsequence>();
            var newPositions = new List<Position>();
            var newSequenceAttributes = new List<SequenceAttribute>();

            for (int i = 1; i < features.Count; i++)
            {
                var feature = features[i];
                var location = feature.Location;
                var leafLocations = location.GetLeafLocations();
                int featureId;

                if (feature.Key == gene)
                {
                    if (!GeneNeedsImport(leafLocations, i))
                    {
                        continue;
                    }

                    featureId = Aliases.Feature.Gene;
                }
                else
                {
                    featureId = featureRepository.GetFeatureIdByName(feature.Key);
                }

                if (feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudo)) ||
                    feature.Qualifiers.ContainsKey(attributeRepository.GetAttributeNameById(Aliases.Attribute.Pseudogene)))
                {
                    featureId = Aliases.Feature.PseudoGen;
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

                AddPositionToMap(start, end);

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

                    AddPositionToMap(leafStart, leafEnd);
                }

                newSequenceAttributes.AddRange(sequenceAttributeRepository.CreateSubsequenceAttributes(feature.Qualifiers, complement, complementJoin, subsequence));
            }

            newSubsequences.AddRange(CreateNonCodingSubsequences());

            db.Subsequence.AddRange(newSubsequences);
            db.Position.AddRange(newPositions);
            db.SequenceAttribute.AddRange(newSequenceAttributes);

            db.SaveChanges();
        }

        /// <summary>
        /// Checks if gene needs import.
        /// </summary>
        /// <param name="leafLocations">
        /// The leaf locations.
        /// </param>
        /// <param name="index">
        /// Index of feature to check.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool GeneNeedsImport(List<ILocation> leafLocations, int index)
        {
            if (leafLocations.Count > 1)
            {
                // don't need to import this gene
                return false;
            }

            var leafLocation = leafLocations[0];

            // if feature is not last
            if ((index + 1) < features.Count)
            {
                var nextLeafLocations = features[index + 1].Location.GetLeafLocations();

                // if there is join in child record parent record contains only
                // first child start and last child end
                if (nextLeafLocations.Count > 1 && leafLocation.LocationStart == nextLeafLocations[0].LocationStart
                    && leafLocation.LocationEnd == nextLeafLocations[nextLeafLocations.Count - 1].LocationEnd)
                {
                    // don't need to import this gene
                    return false;
                }
            }

            // checking if there is any feature with identical position
            if (allNonGenesLeafLocations.Any(l => leafLocation.LocationStart == l.LocationStart &&
                                                  leafLocation.LocationEnd == l.LocationEnd &&
                                                  leafLocation.StartData == l.StartData &&
                                                  leafLocation.EndData == l.EndData))
            {
                // don't need to import this gene
                return false;
            }

            // there are no "children" features with identical position
            return true;
        }

        /// <summary>
        /// Creates non coding subsequences.
        /// </summary>
        /// <returns>
        /// The <see cref="List{Subsequence}"/>.
        /// </returns>
        private List<Subsequence> CreateNonCodingSubsequences()
        {
            var positions = ExtractNonCodingSubsequencesPositions();
            var starts = positions[0];
            var lengths = positions[1];
            var result = new List<Subsequence>();

            for (int i = 0; i < lengths.Count; i++)
            {
                result.Add(new Subsequence
                {
                    Id = DbHelper.GetNewElementId(db),
                    FeatureId = Aliases.Feature.NonCodingSequence,
                    Partial = false,
                    SequenceId = sequenceId,
                    Start = starts[i],
                    Length = lengths[i]
                });
            }

            return result;
        }

        /// <summary>
        /// The add position to map.
        /// </summary>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        private void AddPositionToMap(int start, int end)
        {
            for (int j = start; j <= end; j++)
            {
                positionsMap[j] = true;
            }
        }

        /// <summary>
        /// Extracts non coding subsequences positions
        /// from filled positions map.
        /// </summary>
        /// <returns>
        /// The <see cref="T:List{Int32}[]"/>.
        /// </returns>
        private List<int>[] ExtractNonCodingSubsequencesPositions()
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
            return leafLocations.Any(leafLocation => leafLocation.LocationStart.ToString() != leafLocation.StartData ||
                                                     leafLocation.LocationEnd.ToString() != leafLocation.EndData);
        }
    }
}
