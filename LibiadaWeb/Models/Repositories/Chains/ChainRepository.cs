using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

using Npgsql;

namespace LibiadaWeb.Models.Repositories.Chains
{
    using LibiadaCore.Core;

    public class ChainRepository : ChainImporter, IChainRepository
    {
        private readonly ElementRepository elementRepository;

        public ChainRepository(LibiadaWebEntities db) : base(db)
        {
            elementRepository = new ElementRepository(db);
        }

        public void Insert(chain chain, long[] alphabet, int[] building)
        {
            var parameters = FillParams(chain, alphabet, building);

            const string query = @"INSERT INTO chain (
                                        id, 
                                        notation_id,
                                        matter_id, 
                                        dissimilar, 
                                        piece_type_id, 
                                        piece_position, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id
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
                                        @remote_db_id
                                    );";
            db.Database.ExecuteSqlCommand(query, parameters.ToArray());
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> selectedChains)
        {
            return GetSelectListItems(null, selectedChains);
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> allChains, IEnumerable<chain> selectedChains)
        {
            if (allChains == null)
            {
                allChains = db.chain.Include("matter");
            }
            HashSet<long> chainIds = selectedChains != null
                                          ? new HashSet<long>(selectedChains.Select(c => c.id))
                                          : new HashSet<long>();
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

        public List<element> GetElements(long chainId)
        {
            
            List<long> elementIds = GetElementIds(chainId);
            return elementRepository.GetElements(elementIds);
        }

        public Alphabet GetAlphabet(long chainId)
        {
            List<long> elements = GetElementIds(chainId);
            return elementRepository.ToLibiadaAlphabet(elements);
        }

        public List<long> GetElementIds(long chainId)
        {
            const string Query = "SELECT unnest(alphabet) FROM chain WHERE id = @id";
            return db.Database.SqlQuery<long>(Query, new NpgsqlParameter("@id", chainId)).ToList();
        }

        public int[] GetBuilding(long chainId)
        {
            const string Query = "SELECT unnest(building) FROM chain WHERE id = @id";
            return db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", chainId)).ToArray();
        }

        public BaseChain ToLBaseChain(long chainId)
        {
            return new BaseChain(GetBuilding(chainId), GetAlphabet(chainId));
        }

        public Chain ToLibiadaChain(long chainId)
        {
            return new Chain(GetBuilding(chainId), GetAlphabet(chainId));
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}