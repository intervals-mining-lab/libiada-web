namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Bio;
    using Bio.IO.GenBank;

    using LibiadaCore.Core;
    using LibiadaCore.Misc;

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
                if (location.SubLocations.Count > 0)
                {
                    complement = complement || location.SubLocations[0].Operator == LocationOperator.Complement;

                    var subLocationOperator = location.SubLocations[0].Operator;

                    foreach (var subLocation in location.SubLocations)
                    {
                        if (subLocation.Operator != subLocationOperator)
                        {
                            throw new Exception("SubLocation operators does not match: " + subLocationOperator + " and " + subLocation.Operator);
                        }
                    }
                }

                if (leafLocations[0].LocationStart.ToString() != leafLocations[0].StartData || leafLocations[0].LocationEnd.ToString() != leafLocations[0].EndData)
                {
                    throw new Exception("Location and location data are not equal: location start = " + leafLocations[0].LocationStart
                                                                               + " start data = " + leafLocations[0].StartData
                                                                               + " location end = " + leafLocations[0].LocationEnd
                                                                               + " end data = " + leafLocations[0].EndData);
                }

                int start = leafLocations[0].LocationStart - 1;
                int end = leafLocations[0].LocationEnd - 1;
                int length = end - start + 1;

                if (length < 1)
                {
                    throw new Exception("Length of subsequence cant be less than 1.");
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

                AddStartAndEnd(starts, ends, start, end);

                for (int k = 1; k > leafLocations.Count; k++)
                {
                    var leafLocation = leafLocations[k];

                    if (leafLocation.LocationStart.ToString() != leafLocation.StartData || leafLocation.LocationEnd.ToString() != leafLocation.EndData)
                    {
                        throw new Exception("Location and location data are not equal: location start = " + leafLocation.LocationStart 
                                                                                   + " start data = " + leafLocation.StartData
                                                                                   + " location end = " + leafLocation.LocationEnd
                                                                                   + " end data = " + leafLocation.EndData);
                    }

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

                    AddStartAndEnd(starts, ends, leafStart, leafEnd);
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
            if (!ArrayManipulator.IsSorted(starts) || !ArrayManipulator.IsSorted(ends))
            {
                throw new Exception("Wrong subsequences order.");
            }

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
            List<int> starts;
            List<int> ends;

            CreateFeatureSubsequences(features, sequenceId, out starts, out ends);
            CreateNonCodingSubsequences(starts, ends, sequenceId);

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
        /// The add start and end.
        /// </summary>
        /// <param name="starts">
        /// The starts.
        /// </param>
        /// <param name="ends">
        /// The ends.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if current end is less then previous.
        /// </exception>
        private void AddStartAndEnd(List<int> starts, List<int> ends, int start, int end) 
        {
            starts.Add(start - 1);
            ends.Add(end + 1);
        }
    }
}
