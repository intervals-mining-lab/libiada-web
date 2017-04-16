namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The DNA sequence repository.
    /// </summary>
    public class DnaSequenceRepository : SequenceImporter, IDnaSequenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnaSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public DnaSequenceRepository(LibiadaWebEntities db) : base(db)
        {
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

            var alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.Notation, false);
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
            var parameters = FillParams(sequence, alphabet, building);

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
