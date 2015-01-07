namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// The literature chain repository.
    /// </summary>
    public class LiteratureSequenceRepository : CommonSequenceImporter, ILiteratureSequenceRepository
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
        /// The insert.
        /// </summary>
        /// <param name="commonSequence">
        /// The chain.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="translatorId">
        /// The translator id.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Insert(CommonSequence commonSequence, bool original, int languageId, int? translatorId, long[] alphabet, int[] building)
        {
            var parameters = FillParams(commonSequence, alphabet, building);

            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "original", 
                NpgsqlDbType = NpgsqlDbType.Boolean, 
                Value = original
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "language_id", 
                NpgsqlDbType = NpgsqlDbType.Integer, 
                Value = languageId
            });

            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "translator_id", 
                NpgsqlDbType = NpgsqlDbType.Integer, 
                Value = translatorId
            });

            const string Query = @"INSERT INTO literature_chain (
                                        id, 
                                        notation_id,
                                        matter_id,
                                        piece_type_id, 
                                        piece_position, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id,
                                        original,
                                        language_id,
                                        translator_id
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id,
                                        @piece_type_id, 
                                        @piece_position, 
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id,
                                        @original,
                                        @language_id,
                                        @translator_id
                                    );";
            db.Database.ExecuteSqlCommand(Query, parameters.ToArray());
        }

        /// <summary>
        /// The to chain.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="CommonSequence"/>.
        /// </returns>
        public CommonSequence ToCommonSequence(LiteratureSequence source)
        {
            return new CommonSequence
            {
                Id = source.Id,
                NotationId = source.NotationId, 
                MatterId = source.MatterId, 
                PieceTypeId = source.PieceTypeId, 
                PiecePosition = source.PiecePosition
            };
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="sequences">
        /// The chains.
        /// </param>
        /// <returns>
        /// The <see cref="List{SelectListItem}"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<LiteratureSequence> sequences)
        {
            HashSet<long> chainIds = sequences != null
                                         ? new HashSet<long>(sequences.Select(c => c.Id))
                                         : new HashSet<long>();
            var allSequences = db.LiteratureSequence.Include("matter");
            var chainsList = new List<SelectListItem>();
            foreach (var sequence in allSequences)
            {
                chainsList.Add(new SelectListItem
                    {
                        Value = sequence.Id.ToString(), 
                        Text = sequence.Matter.Name, 
                        Selected = chainIds.Contains(sequence.Id)
                    });
            }

            return chainsList;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
