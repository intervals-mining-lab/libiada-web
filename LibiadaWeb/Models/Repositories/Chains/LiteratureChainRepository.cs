namespace LibiadaWeb.Models.Repositories.Chains
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The literature chain repository.
    /// </summary>
    public class LiteratureChainRepository : ChainImporter, ILiteratureChainRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteratureChainRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public LiteratureChainRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="chain">
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
        public void Insert(chain chain, bool original, int languageId, int? translatorId, long[] alphabet, int[] building)
        {
            var parameters = this.FillParams(chain, alphabet, building);

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
        /// The <see cref="chain"/>.
        /// </returns>
        public chain ToChain(literature_chain source)
        {
            return new chain
            {
                id = source.id,
                notation_id = source.notation_id, 
                matter_id = source.matter_id, 
                piece_type_id = source.piece_type_id, 
                piece_position = source.piece_position
            };
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="chains">
        /// The chains.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<literature_chain> chains)
        {
            HashSet<long> chainIds = chains != null
                                         ? new HashSet<long>(chains.Select(c => c.id))
                                         : new HashSet<long>();
            var allChains = db.literature_chain.Include("matter");
            var chainsList = new List<SelectListItem>();
            foreach (var chain in allChains)
            {
                chainsList.Add(new SelectListItem
                    {
                        Value = chain.id.ToString(), 
                        Text = chain.matter.name, 
                        Selected = chainIds.Contains(chain.id)
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