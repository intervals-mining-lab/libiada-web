using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Npgsql;
using NpgsqlTypes;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class LiteratureChainRepository : ChainImporter, ILiteratureChainRepository
    {
        public LiteratureChainRepository(LibiadaWebEntities db) : base(db)
        {
        }

        public void Insert(chain chain, bool original, int languageId, int? translatorId, long[] alphabet, int[] building)
        {
            var parameters = FillParams(chain, alphabet, building);

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
                                        dissimilar, 
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
                                        @dissimilar, 
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

        public chain ToChain(literature_chain source)
        {
            return new chain
            {
                id = source.id,
                dissimilar = source.dissimilar,
                notation_id = source.notation_id,
                matter_id = source.matter_id,
                piece_type_id = source.piece_type_id,
                piece_position = source.piece_position
            };
        }

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

        public void Dispose()
        {
            db.Dispose();
        }
    }
}