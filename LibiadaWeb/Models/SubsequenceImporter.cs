﻿namespace LibiadaWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Bio.IO.GenBank;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;

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
        /// The attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

        /// <summary>
        /// The sequence id.
        /// </summary>
        private readonly long sequenceId;

        /// <summary>
        /// The features.
        /// </summary>
        private readonly List<FeatureItem> features;

        /// <summary>
        /// The all non genes leaf locations.
        /// </summary>
        private readonly List<ILocation>[] allNonGenesLeafLocations;

        /// <summary>
        /// Boolean map of filled positions in full sequence.
        /// </summary>
        private readonly bool[] positionsMap;

        /// <summary>
        /// Gene feature type name.
        /// </summary>
        private readonly string gene = Feature.Gene.GetGenBankName();

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceImporter"/> class.
        /// </summary>
        /// <param name="sequence">
        /// Dna sequence for which subsequences will be imported.
        /// </param>
        public SubsequenceImporter(DnaSequence sequence) : this(NcbiHelper.GetFeatures(sequence.RemoteId), sequence.Id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceImporter"/> class.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <exception cref="Exception">
        /// thrown if length of sequence from database
        /// is not equal to the length of downloaded sequence.
        /// </exception>
        public SubsequenceImporter(List<FeatureItem> features, long sequenceId)
        {
            this.features = features;
            this.sequenceId = sequenceId;
            sequenceAttributeRepository = new SequenceAttributeRepository(db);

            allNonGenesLeafLocations = features.Where(f => f.Key != gene)
                                               .Select(f => f.Location.GetLeafLocations())
                                               .ToArray();

            int parentLength = db.GetSequenceLength(sequenceId);
            int sourceLength = features[0].Location.LocationEnd;
            positionsMap = new bool[parentLength];

            if (parentLength != sourceLength)
            {
                throw new Exception($"Local and loaded sequence length are not equal. Local length: {parentLength}, loaded length: {sourceLength}");
            }
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
        /// <returns>
        /// Returns tuple of coding and non-coding features count.
        /// </returns>
        public (int, int) CreateSubsequences()
        {
            try
            {
                CheckImportability();
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred during importability check.", e);
            }

            return CreateFeatureSubsequences();
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
            for (int i = 1; i < features.Count; i++)
            {
                FeatureItem feature = features[i];
                ILocation location = feature.Location;
                List<ILocation> leafLocations = location.GetLeafLocations();

                if (feature.Key == "source")
                {
                    throw new Exception($"Sequence seems to be chimeric as it is several 'source' records in file. Second source location: {leafLocations[0].StartData}");
                }

                if (!FeatureRepository.FeatureExists(feature.Key))
                {
                    throw new Exception($"Unknown feature. Feature name = {feature.Key}");
                }

                if (feature.Key == gene)
                {
                    // checking if there is any feature with identical location
                    if (allNonGenesLeafLocations.Any(l => LocationsEqual(leafLocations, l)))
                    {
                        continue;
                    }
                }

                if (location.SubLocations.Count > 0)
                {
                    LocationOperator subLocationOperator = location.SubLocations[0].Operator;

                    foreach (ILocation subLocation in location.SubLocations)
                    {
                        if (subLocation.Operator != subLocationOperator)
                        {
                            throw new Exception($"SubLocation operators does not match: {subLocationOperator} and {subLocation.Operator}");
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
        /// Checks if gene have equal location with another feature.
        /// </summary>
        /// <param name="geneLocation">
        /// The gene leaf locations.
        /// </param>
        /// <param name="otherLocation">
        /// The other feature leaf locations.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool LocationsEqual(List<ILocation> geneLocation, List<ILocation> otherLocation)
        {
            // if there is a join in child record parent record contains only
            // first child start and last child end
            if (geneLocation.Count == 1
             && geneLocation[0].LocationStart == otherLocation[0].LocationStart
             && geneLocation[0].LocationEnd == otherLocation[otherLocation.Count - 1].LocationEnd)
            {
                return true;
            }

            // if gene is multi-positional
            if (geneLocation.Count != otherLocation.Count)
            {
                return false;
            }

            for (int i = 0; i < geneLocation.Count; i++)
            {
                // if any sublocations are not equal
                if (geneLocation[i].LocationStart != otherLocation[i].LocationStart
                 || geneLocation[i].LocationEnd != otherLocation[i].LocationEnd)
                {
                    return false;
                }
            }

            // if all sublocations are equal
            return true;
        }

        /// <summary>
        /// Create subsequences from features
        /// and non-coding subsequences from gaps.
        /// </summary>
        /// <returns>
        /// Returns tuple of coding and non-coding features count.
        /// </returns>
        private (int, int) CreateFeatureSubsequences()
        {
            var codingSubsequences = new List<Subsequence>(features.Count);
            var newPositions = new List<Position>();
            var newSequenceAttributes = new List<SequenceAttribute>();

            for (int i = 1; i < features.Count; i++)
            {
                FeatureItem feature = features[i];
                ILocation location = feature.Location;
                List<ILocation> leafLocations = location.GetLeafLocations();
                Feature subsequenceFeature;

                if (feature.Key == gene)
                {
                    if (allNonGenesLeafLocations.Any(l => LocationsEqual(leafLocations, l)))
                    {
                        continue;
                    }

                    subsequenceFeature = Feature.Gene;
                }
                else
                {
                    subsequenceFeature = FeatureRepository.GetFeatureByName(feature.Key);
                }

                if (feature.Qualifiers.ContainsKey(LibiadaWeb.Attribute.Pseudo.GetDisplayValue())
                 || feature.Qualifiers.ContainsKey(LibiadaWeb.Attribute.Pseudogene.GetDisplayValue()))
                {
                    subsequenceFeature = Feature.PseudoGen;
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
                    Id = db.GetNewElementId(),
                    Feature = subsequenceFeature,
                    Partial = partial,
                    SequenceId = sequenceId,
                    Start = start,
                    Length = length,
                    RemoteId = location.Accession
                };

                codingSubsequences.Add(subsequence);
                AddPositionToMap(start, end);
                newPositions.AddRange(CreateAdditionalPositions(leafLocations, subsequence.Id));
                var sequenceAttributes = sequenceAttributeRepository.Create(feature.Qualifiers, complement, complementJoin, subsequence);
                newSequenceAttributes.AddRange(sequenceAttributes);
            }

            var nonCodingSubsequences = CreateNonCodingSubsequences();

            db.Subsequence.AddRange(codingSubsequences);
            db.Subsequence.AddRange(nonCodingSubsequences);
            db.Position.AddRange(newPositions);
            db.SequenceAttribute.AddRange(newSequenceAttributes);

            db.SaveChanges();

            return (codingSubsequences.Count, nonCodingSubsequences.Count);
        }

        /// <summary>
        /// The create additional positions.
        /// </summary>
        /// <param name="leafLocations">
        /// The leaf locations.
        /// </param>
        /// <param name="subsequenceId">
        /// The subsequence id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Position}"/>.
        /// </returns>
        private List<Position> CreateAdditionalPositions(List<ILocation> leafLocations, long subsequenceId)
        {
            var result = new List<Position>(leafLocations.Count - 1);

            for (int k = 1; k < leafLocations.Count; k++)
            {
                ILocation leafLocation = leafLocations[k];
                int leafStart = leafLocation.LocationStart - 1;
                int leafEnd = leafLocation.LocationEnd - 1;
                int leafLength = leafEnd - leafStart + 1;

                result.Add(new Position { SubsequenceId = subsequenceId, Start = leafStart, Length = leafLength });
                AddPositionToMap(leafStart, leafEnd);
            }

            return result;
        }

        /// <summary>
        /// Creates non coding subsequences.
        /// </summary>
        /// <returns>
        /// The <see cref="List{Subsequence}"/>.
        /// </returns>
        private List<Subsequence> CreateNonCodingSubsequences()
        {
            List<NonCodingPosition> positions = ExtractNonCodingSubsequencesPositions();
            return positions.ConvertAll(p => new Subsequence
                                                 {
                                                     Id = db.GetNewElementId(),
                                                     Feature = Feature.NonCodingSequence,
                                                     Partial = false,
                                                     SequenceId = sequenceId,
                                                     Start = p.Start,
                                                     Length = p.Length
                                                 });
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
        /// The <see cref="T:List{NonCodingPosition}"/>.
        /// </returns>
        private List<NonCodingPosition> ExtractNonCodingSubsequencesPositions()
        {
            var map = new List<NonCodingPosition>();
            var currentStart = 0;
            var currentLength = 0;

            for (int i = 0; i < positionsMap.Length; i++)
            {
                if (positionsMap[i])
                {
                    if (currentLength > 0)
                    {
                        map.Add(new NonCodingPosition(currentStart, currentLength));
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
                map.Add(new NonCodingPosition(currentStart, currentLength));
            }

            return map;
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

        /// <summary>
        /// The non coding position start and length.
        /// </summary>
        private struct NonCodingPosition
        {
            /// <summary>
            /// The start.
            /// </summary>
            public readonly int Start;

            /// <summary>
            /// The length.
            /// </summary>
            public readonly int Length;

            /// <summary>
            /// Initializes a new instance of the <see cref="NonCodingPosition"/> structure.
            /// </summary>
            /// <param name="start">
            /// The start.
            /// </param>
            /// <param name="length">
            /// The length.
            /// </param>
            public NonCodingPosition(int start, int length)
            {
                Start = start;
                Length = length;
            }
        }
    }
}
