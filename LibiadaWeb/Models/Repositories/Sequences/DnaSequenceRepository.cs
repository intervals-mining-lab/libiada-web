namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Text;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The dna sequence repository.
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
        /// The create dna sequence.
        /// </summary>
        /// <param name="sequence">
        /// The common sequence.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <param name="complementary">
        /// The complementary.
        /// </param>
        /// <param name="stringSequence">
        /// The string sequence.
        /// </param>
        /// <param name="webApiId">
        /// The web api id.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if at least one element of new sequence is missing in db
        /// or if sequence is empty or invalid.
        /// </exception>
        public void Create(CommonSequence sequence, bool partial, bool complementary, string stringSequence, int? webApiId)
        {
            // separating fasta header from sequence.
            string[] splittedFasta = stringSequence.Split('\n', '\r');
            var sequenceStringBuilder = new StringBuilder();
            string fastaHeader = splittedFasta[0];

            if (fastaHeader.Contains("Resource temporarily unavailable") || splittedFasta.Length < 2)
            {
                throw new Exception("Sequence is empty or invalid (probably ncbi is not responding).");
            }

            for (int j = 1; j < splittedFasta.Length; j++)
            {
                sequenceStringBuilder.Append(splittedFasta[j]);
            }

            string resultStringSequence = DataTransformers.CleanFastaFile(sequenceStringBuilder.ToString());

            var chain = new BaseChain(resultStringSequence);

            if (!ElementRepository.ElementsInDb(chain.Alphabet, sequence.NotationId))
            {
                throw new Exception("At least one element of new sequence is missing in db.");
            }

            MatterRepository.CreateMatterFromSequence(sequence);

            var alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.NotationId, false);
            Create(sequence, fastaHeader, webApiId, complementary, partial, alphabet, chain.Building);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="fastaHeader">
        /// The fasta header.
        /// </param>
        /// <param name="webApiId">
        /// The web api id.
        /// </param>
        /// <param name="complementary">
        /// The complementary.
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
        public void Create(CommonSequence sequence, string fastaHeader, int? webApiId, bool complementary, bool partial, long[] alphabet, int[] building)
        {
            var parameters = FillParams(sequence, alphabet, building);
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "fasta_header", 
                NpgsqlDbType = NpgsqlDbType.Varchar, 
                Value = fastaHeader
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "web_api_id", 
                NpgsqlDbType = NpgsqlDbType.Integer, 
                Value = webApiId
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "partial", 
                NpgsqlDbType = NpgsqlDbType.Boolean, 
                Value = partial
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "complementary", 
                NpgsqlDbType = NpgsqlDbType.Boolean,
                Value = complementary
            });

            const string Query = @"INSERT INTO dna_chain (
                                        id, 
                                        notation_id,
                                        matter_id, 
                                        feature_id, 
                                        piece_position, 
                                        fasta_header, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id, 
                                        web_api_id,
                                        partial,
                                        complementary
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id,
                                        @feature_id, 
                                        @piece_position, 
                                        @fasta_header, 
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id, 
                                        @web_api_id,
                                        @partial,
                                        @complementary
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
            Create(ToCommonSequence(sequence), sequence.FastaHeader, sequence.WebApiId, false, false, alphabet, building);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            Db.Dispose();
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
                NotationId = source.NotationId, 
                MatterId = source.MatterId, 
                FeatureId = source.FeatureId, 
                PiecePosition = source.PiecePosition
            };
        }
    }
}
