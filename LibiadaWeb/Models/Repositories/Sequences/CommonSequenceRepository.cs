namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using LibiadaCore.Core;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    public class CommonSequenceRepository : SequenceImporter, ICommonSequenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CommonSequenceRepository(LibiadaWebEntities db) : base(db)
        {
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
        public void Create(CommonSequence sequence, long[] alphabet, int[] building)
        {
            var parameters = FillParams(sequence, alphabet, building);

            const string Query = @"INSERT INTO chain (
                                        id,
                                        notation,
                                        matter_id,
                                        alphabet,
                                        building,
                                        remote_id,
                                        remote_db
                                    ) VALUES (
                                        @id,
                                        @notation,
                                        @matter_id,
                                        @alphabet,
                                        @building,
                                        @remote_id,
                                        @remote_db
                                    );";

            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
        }

        /// <summary>
        /// The get elements.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="List{element}"/>.
        /// </returns>
        public List<Element> GetElements(long sequenceId)
        {
            long[] elementIds = DbHelper.GetAlphabetElementIds(Db, sequenceId);
            return ElementRepository.GetElements(elementIds);
        }

        /// <summary>
        /// Loads sequence by id from db and converts it to <see cref="BaseChain"/>.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The sequence as <see cref="BaseChain"/>.
        /// </returns>
        public BaseChain GetLibiadaBaseChain(long sequenceId)
        {
            return new BaseChain(DbHelper.GetSequenceBuilding(Db, sequenceId), GetAlphabet(sequenceId), sequenceId);
        }

        /// <summary>
        /// Loads sequence by id from db and converts it to <see cref="Chain"/>.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The sequence as <see cref="Chain"/>.
        /// </returns>
        public Chain GetLibiadaChain(long sequenceId)
        {
            return new Chain(DbHelper.GetSequenceBuilding(Db, sequenceId), GetAlphabet(sequenceId), sequenceId);
        }

        /// <summary>
        /// Loads sequence by id from db and converts it to <see cref="BaseChain"/>.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The sequence as <see cref="BaseChain"/>.
        /// </returns>
        public string GetString(long sequenceId)
        {
            int[] order = DbHelper.GetSequenceBuilding(Db, sequenceId);
            Alphabet alphabet = GetAlphabet(sequenceId);
            var stringBuilder = new StringBuilder(order.Length);
            foreach (int element in order)
            {
                stringBuilder.Append(alphabet[element]);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Extracts sequences ids from database.
        /// </summary>
        /// <param name="matterIds">
        /// The matters ids.
        /// </param>
        /// <param name="notations">
        /// The notations ids.
        /// </param>
        /// <param name="languages">
        /// The languages ids.
        /// </param>
        /// <param name="translators">
        /// The translators ids.
        /// </param>
        /// <returns>
        /// The sequences ids as <see cref="T:long[][]"/>.
        /// </returns>
        public long[][] GetSequenceIds(long[] matterIds, Notation[] notations, Language[] languages, Translator?[] translators)
        {
            var sequenceIds = new long[matterIds.Length][];

            for (int i = 0; i < matterIds.Length; i++)
            {
                var matterId = matterIds[i];
                sequenceIds[i] = new long[notations.Length];

                for (int j = 0; j < notations.Length; j++)
                {
                    Notation notation = notations[j];

                    if (notation.GetNature() == Nature.Literature)
                    {
                        Language language = languages[j];
                        Translator translator = translators[j] ?? Translator.NoneOrManual;

                        sequenceIds[i][j] = Db.LiteratureSequence.Single(l => l.MatterId == matterId
                                                                           && l.Notation == notation
                                                                           && l.Language == language
                                                                           && l.Translator == translator).Id;
                    }
                    else
                    {
                        sequenceIds[i][j] = Db.CommonSequence.Single(c => c.MatterId == matterId && c.Notation == notation).Id;
                    }
                }
            }

            return sequenceIds;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// The get alphabet.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="Alphabet"/>.
        /// </returns>
        private Alphabet GetAlphabet(long sequenceId)
        {
            long[] elements = DbHelper.GetAlphabetElementIds(Db, sequenceId);
            return ElementRepository.ToLibiadaAlphabet(elements);
        }
    }
}
