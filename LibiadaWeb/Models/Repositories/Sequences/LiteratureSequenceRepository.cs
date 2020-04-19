namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
        public void Create(CommonSequence commonSequence, Stream sequenceStream, Language language, bool original, Translator translator, bool dropPunctuation = false)
        {
            string stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);
            BaseChain chain;
            if (commonSequence.Notation == Notation.Letters)
            {
                stringSequence = stringSequence.ToUpper();
                if (dropPunctuation)
                {
                    stringSequence = new string(stringSequence.Where(c => !char.IsPunctuation(c)).ToArray());
                }
                chain = new BaseChain(stringSequence);
            }
            else
            {
                // file always contains empty string at the end
                // TODO: rewrite this, add empty string check at the end or write a normal trim
                string[] text = stringSequence.Split(new[] { '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                chain = new BaseChain(text.Select(e => (ValueString)e).Cast<IBaseObject>().ToList());
            }

            MatterRepository.CreateOrExtractExistingMatterForSequence(commonSequence);

            long[] alphabet = ElementRepository.ToDbElements(chain.Alphabet, commonSequence.Notation, true);
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
            List<NpgsqlParameter> parameters = FillParams(sequence, alphabet, building);
            parameters.Add(new NpgsqlParameter<bool>("original", NpgsqlDbType.Boolean) { TypedValue = original });
            parameters.Add(new NpgsqlParameter<byte>("language", NpgsqlDbType.Smallint) { TypedValue = (byte)language });
            parameters.Add(new NpgsqlParameter<byte>("translator", NpgsqlDbType.Smallint) { TypedValue = (byte)translator });

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

            Db.ExecuteCommand(Query, parameters.ToArray());
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
