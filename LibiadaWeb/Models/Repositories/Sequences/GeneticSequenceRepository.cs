namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The DNA sequence repository.
    /// </summary>
    public class GeneticSequenceRepository : SequenceImporter, IGeneticSequenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public GeneticSequenceRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// Compares two sequence accessions.
        /// Takes into account their versions only if they has one.
        /// </summary>
        /// <param name="firstAccession">
        /// The first accession.
        /// </param>
        /// <param name="secondAccession">
        /// The second accession.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CompareAccessions(string firstAccession, string secondAccession)
        {
            string[] splitFirstAccession = firstAccession.Split('.');
            string[] splitSecondAccession = secondAccession.Split('.');

            // comparing accessions without versions
            if (!splitFirstAccession[0].Equals(splitSecondAccession[0]))
            {
                return false;
            }

            // checking if both accessions have version
            if (splitFirstAccession.Length == 2 && splitSecondAccession.Length == 2)
            {
                // comparing versions
                return splitFirstAccession[1].Equals(splitSecondAccession[1]);
            }

            return true;
        }

        /// <summary>
        /// Splits given accessions into existing and not imported.
        /// </summary>
        /// <param name="accessions">
        /// The accessions.
        /// </param>
        /// <returns>
        /// First array in tuple are existing accessions and second array are not imported accessions.
        /// </returns>
        public (string[], string[]) SplitAccessionsIntoExistingAndNotImported(string[] accessions)
        {
            string[] allExistingAccessions = Db.DnaSequence.Select(d => d.RemoteId).Distinct().ToArray();
            var result = (new List<string>(), new List<string>());

            foreach (string accession in accessions)
            {
                if (allExistingAccessions.Any(aa => CompareAccessions(aa, accession)))
                {
                    result.Item1.Add(accession);
                }
                else
                {
                    result.Item2.Add(accession);
                }
            }

            return (result.Item1.ToArray(), result.Item2.ToArray());
        }

        /// <summary>
        /// The create DNA sequence.
        /// </summary>
        /// <param name="sequence">
        /// The common sequence.
        /// </param>
        /// <param name="fastaSequence">
        /// Sequence as <see cref="ISequence"/>>.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if at least one element of new sequence is missing in db
        /// or if sequence is empty or invalid.
        /// </exception>
        public void Create(CommonSequence sequence, ISequence fastaSequence, bool partial)
        {
            if (fastaSequence.ID.Contains("Resource temporarily unavailable"))
            {
                throw new Exception("Sequence is empty or invalid (probably ncbi is not responding).");
            }

            string stringSequence = fastaSequence.ConvertToString().ToUpper();

            var chain = new BaseChain(stringSequence);

            if (!ElementRepository.ElementsInDb(chain.Alphabet, sequence.Notation))
            {
                throw new Exception("At least one element of new sequence is invalid (not A, C, T, G or U).");
            }

            MatterRepository.CreateMatterFromSequence(sequence);

            long[] alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.Notation, false);
            Create(sequence, partial, alphabet, chain.Building);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Create(CommonSequence sequence, bool partial, long[] alphabet, int[] building)
        {
            List<object> parameters = FillParams(sequence, alphabet, building);

            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "partial",
                NpgsqlDbType = NpgsqlDbType.Boolean,
                Value = partial
            });

            const string Query = @"INSERT INTO dna_chain (
                                        id,
                                        notation,
                                        matter_id,
                                        alphabet,
                                        building,
                                        remote_id,
                                        remote_db,
                                        partial
                                    ) VALUES (
                                        @id,
                                        @notation,
                                        @matter_id,
                                        @alphabet,
                                        @building,
                                        @remote_id,
                                        @remote_db,
                                        @partial
                                    );";

            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Insert(DnaSequence sequence, long[] alphabet, int[] building)
        {
            Create(ToCommonSequence(sequence), false, alphabet, building);
        }

        /// <summary>
        /// Extracts nucleotide sequences ids from database.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:long[]"/>.
        /// </returns>
        public long[] GetNucleotideSequenceIds(long[] matterIds)
        {
            var chains = new long[matterIds.Length];
            DnaSequence[] sequences = Db.DnaSequence.Where(c => matterIds.Contains(c.MatterId) && c.Notation == Notation.Nucleotides).ToArray();
            for (int i = 0; i < matterIds.Length; i++)
            {
                chains[i] = sequences.Single(c => c.MatterId == matterIds[i]).Id;
            }

            return chains;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// The to sequence.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="CommonSequence"/>.
        /// </returns>
        private CommonSequence ToCommonSequence(DnaSequence source)
        {
            return new CommonSequence
                       {
                           Id = source.Id,
                           Notation = source.Notation,
                           MatterId = source.MatterId
                       };
        }
    }
}
