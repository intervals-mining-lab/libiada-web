namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;

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
                                        notation_id,
                                        matter_id,
                                        feature_id,
                                        alphabet,
                                        building,
                                        remote_id,
                                        remote_db_id
                                    ) VALUES (
                                        @id,
                                        @notation_id,
                                        @matter_id,
                                        @feature_id,
                                        @alphabet,
                                        @building,
                                        @remote_id,
                                        @remote_db_id
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
            List<long> elementIds = DbHelper.GetElementIds(Db, sequenceId);
            return ElementRepository.GetElements(elementIds);
        }

        /// <summary>
        /// The to libiada BaseChain.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="BaseChain"/>.
        /// </returns>
        public BaseChain ToLibiadaBaseChain(long sequenceId)
        {
            return new BaseChain(DbHelper.GetBuilding(Db, sequenceId), GetAlphabet(sequenceId), sequenceId);
        }

        /// <summary>
        /// The to libiada Chain.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="Chain"/>.
        /// </returns>
        public Chain ToLibiadaChain(long sequenceId)
        {
            return new Chain(DbHelper.GetBuilding(Db, sequenceId), GetAlphabet(sequenceId), sequenceId);
        }

        /// <summary>
        /// Extracts sequences from database.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="languageIds">
        /// The language ids.
        /// </param>
        /// <param name="translatorIds">
        /// The translator ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:Chain[][]"/>.
        /// </returns>
        public Chain[][] GetChains(long[] matterIds, int[] notationIds, int[] languageIds, int?[] translatorIds)
        {
            var chains = new Chain[matterIds.Length][];
            var commonSequenceRepository = new CommonSequenceRepository(Db);
            var notationsRepository = new NotationRepository(Db);

            for (int i = 0; i < matterIds.Length; i++)
            {
                var matterId = matterIds[i];
                chains[i] = new Chain[notationIds.Length];

                for (int j = 0; j < notationIds.Length; j++)
                {
                    Notation notation = notationsRepository.Notations.Single(n => n.Id == notationIds[j]);

                    long sequenceId;
                    if (notation.Nature == Nature.Literature)
                    {
                        int languageId = languageIds[j];
                        int? translatorId = translatorIds[j];

                        sequenceId = Db.LiteratureSequence.Single(l =>
                                    l.MatterId == matterId && l.NotationId == notation.Id
                                    && l.LanguageId == languageId
                                    && ((translatorId == null && l.TranslatorId == null)
                                        || (translatorId == l.TranslatorId))).Id;
                    }
                    else
                    {
                        sequenceId = Db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notation.Id).Id;
                    }

                    chains[i][j] = commonSequenceRepository.ToLibiadaChain(sequenceId);
                }
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
            List<long> elements = DbHelper.GetElementIds(Db, sequenceId);
            return ElementRepository.ToLibiadaAlphabet(elements);
        }
    }
}
