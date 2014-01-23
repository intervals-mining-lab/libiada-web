using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.TheoryOfSet;
using Npgsql;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class ChainRepository :ChainImporter, IChainRepository
    {
        private readonly ElementRepository elementRepository;

        public ChainRepository(LibiadaWebEntities db) : base(db)
        {
            elementRepository = new ElementRepository(db);
        }

        public IQueryable<chain> All
        {
            get { return db.chain; }
        }

        public IQueryable<chain> AllIncluding(params Expression<Func<chain, object>>[] includeProperties)
        {
            IQueryable<chain> query = db.chain;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public chain Find(long id)
        {
            return db.chain.Single(x => x.id == id);
        }

        public void Insert(chain chain, long[] alphabet, int[] building)
        {
            var parameters = FillParams(chain, alphabet, building);

            String query = @"INSERT INTO dna_chain (
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
            db.ExecuteStoreCommand(query, parameters.ToArray());
        }

        public void InsertOrUpdate(chain chain)
        {
            if (chain.id == default(long))
            {
                // New entity
                throw new NotSupportedException("Для добавления новых записей следует использовать метод Insert.");
            }
            else
            {
                // Existing entity
                db.chain.Attach(chain);
                db.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var chain = Find(id);
            db.chain.DeleteObject(chain);
        }

        public void Save()
        {
            db.SaveChanges();
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
            const string query = "SELECT unnest(alphabet) FROM chain WHERE id = @id";
            return db.ExecuteStoreQuery<long>(query, new NpgsqlParameter("@id", chainId)).ToList();
        }

        public int[] GetBuilding(long chainId)
        {
            const string query = "SELECT unnest(building) FROM chain WHERE id = @id";
            return db.ExecuteStoreQuery<int>(query, new NpgsqlParameter("@id", chainId)).ToArray();
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