namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.IO;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The literature sequence repository.
    /// </summary>
    public class LiteratureSequenceRepository : SequenceImporter, ILiteratureSequenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteratureSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public LiteratureSequenceRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// Creates literature sequence in database.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        /// <param name="sequenceStream">
        /// The sequence stream.
        /// </param>
        /// <param name="language">
        /// The language id.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="translator">
        /// The translator id.
        /// </param>
        public void Create(CommonSequence commonSequence, Stream sequenceStream, Language language, bool original, Translator translator)
        {
            var stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);
            string[] text = stringSequence.Split('\n');
            for (int l = 0; l < text.Length - 1; l++)
            {
                // removing "\r"
                text[l] = text[l].Substring(0, text[l].Length - 1);
            }

            var chain = new BaseChain(text.Length - 1);

            // file always contains empty string at the end
            // TODO: rewrite this, add empty string check at the end or write a normal trim
            for (int i = 0; i < text.Length - 1; i++)
            {
                chain.Set(new ValueString(text[i]), i);
            }

            MatterRepository.CreateMatterFromSequence(commonSequence);

            var alphabet = ElementRepository.ToDbElements(chain.Alphabet, commonSequence.Notation, true);
            Create(commonSequence, original, language, translator, alphabet, chain.Building);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="language">
        /// The language id.
        /// </param>
        /// <param name="translator">
        /// The translator id.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Create(CommonSequence sequence, bool original, Language language, Translator translator, long[] alphabet, int[] building)
        {
            var parameters = FillParams(sequence, alphabet, building);

            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "original",
                NpgsqlDbType = NpgsqlDbType.Boolean,
                Value = original
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "language",
                NpgsqlDbType = NpgsqlDbType.Smallint,
                Value = language
            });

            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "translator",
                NpgsqlDbType = NpgsqlDbType.Smallint,
                Value = translator
            });

            const string Query = @"INSERT INTO literature_chain (
                                        id,
                                        notation,
                                        matter_id,
                                        alphabet,
                                        building,
                                        remote_id,
                                        remote_db,
                                        original,
                                        language,
                                        translator
                                    ) VALUES (
                                        @id,
                                        @notation,
                                        @matter_id,
                                        @alphabet,
                                        @building,
                                        @remote_id,
                                        @remote_db,
                                        @original,
                                        @language,
                                        @translator
                                    );";

            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            Db.Dispose();
        }
    }
}
