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
    public class DnaChainRepository : ChainImporter, IDnaChainRepository
    {
        public DnaChainRepository(LibiadaWebEntities db) : base(db)
        {
        }

        public void Insert(
            chain chain,
            string fastaHeader,
            int? webApiId,
            int? productId,
            bool complement,
            bool partial,
            long[] alphabet,
            int[] building)
        {
            var parameters = FillParams(chain, alphabet, building);
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
                ParameterName = "product_id",
                NpgsqlDbType = NpgsqlDbType.Integer,
                Value = productId
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "partial",
                NpgsqlDbType = NpgsqlDbType.Boolean,
                Value = partial
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "complement",
                NpgsqlDbType = NpgsqlDbType.Boolean,
                Value = complement
            });

            const string Query = @"INSERT INTO dna_chain (
                                        id, 
                                        notation_id,
                                        matter_id, 
                                        dissimilar, 
                                        piece_type_id, 
                                        piece_position, 
                                        fasta_header, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id, 
                                        web_api_id,
                                        product_id,
                                        partial,
                                        complement
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id, 
                                        @dissimilar, 
                                        @piece_type_id, 
                                        @piece_position, 
                                        @fasta_header, 
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id, 
                                        @web_api_id,
                                        @product_id,
                                        @partial,
                                        @complement
                                    );";
            db.Database.ExecuteSqlCommand(Query, parameters.ToArray());
        }

        public void Insert(dna_chain chain, long[] alphabet, int[] building)
        {
            Insert(ToChain(chain), chain.fasta_header, chain.web_api_id, null, false, false, alphabet, building);
        }

        public chain ToChain(dna_chain source)
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

        public List<SelectListItem> GetSelectListItems(IEnumerable<dna_chain> chains)
        {
            return GetSelectListItems(db.dna_chain.ToList(), chains);
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<dna_chain> allChains,
            IEnumerable<dna_chain> selectedChain)
        {
            HashSet<long> chainIds = selectedChain != null
                ? new HashSet<long>(selectedChain.Select(c => c.id))
                : new HashSet<long>();
            if (allChains == null)
            {
                allChains = db.dna_chain.Include("matter");
            }
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