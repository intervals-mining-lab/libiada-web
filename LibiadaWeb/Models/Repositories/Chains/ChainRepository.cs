using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.TheoryOfSet;
using LibiadaWeb.Helpers;
using Npgsql;
using NpgsqlTypes;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class ChainRepository : IChainRepository
    {
        private readonly LibiadaWebEntities db;
        private readonly ElementRepository elementRepository;

        public ChainRepository(LibiadaWebEntities db)
        {
            this.db = db;
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
            if (chain.id == 0)
            {
                chain.id = DataTransformators.GetLongSequenceValue(db, "chain_id_seq");
            }

            NpgsqlParameter[] parameters =
                {
                    new NpgsqlParameter
                        {
                            ParameterName = "@id",
                            NpgsqlDbType = NpgsqlDbType.Bigint,
                            Value = chain.id
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@notation_id",
                            NpgsqlDbType = NpgsqlDbType.Integer,
                            Value = chain.notation_id
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@matter_id",
                            NpgsqlDbType = NpgsqlDbType.Bigint,
                            Value = chain.matter_id
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@piece_type_id",
                            NpgsqlDbType = NpgsqlDbType.Integer,
                            Value = chain.piece_type_id
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@alphabet",
                            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Bigint,
                            Value = alphabet
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@building",
                            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer,
                            Value = building
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@creation_date",
                            NpgsqlDbType = NpgsqlDbType.TimestampTZ,
                            Value = DateTime.Now
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@piece_position",
                            NpgsqlDbType = NpgsqlDbType.Integer,
                            Value = chain.piece_position
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@dissimilar",
                            NpgsqlDbType = NpgsqlDbType.Boolean,
                            Value = chain.dissimilar
                        }

                };

            String query =
                "SELECT create_chain(@id,@notation_id,@matter_id,@piece_type_id,@alphabet,@building,@creation_date,@piece_position,@dissimilar);";
            db.ExecuteStoreCommand(query, parameters);
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
            return GetSelectListItems(db.chain.Include("matter"), selectedChains);
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> allChains, IEnumerable<chain> selectedChains)
        {

            HashSet<long> chainIds = selectedChains != null
                                          ? new HashSet<long>(selectedChains.Select(c => c.id))
                                          : new HashSet<long>();
            var chainsList = new List<SelectListItem>();
            if (allChains == null)
            {
                allChains = db.chain.Include("matter");
            }
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

        public List<element> GetChainElements(long chainId)
        {
            
            List<long> elementIds = GetChainElementIds(chainId);
            return elementRepository.GetElements(elementIds);
        }

        public Alphabet GetChainAlphabet(long chainId)
        {
            List<long> elements = GetChainElementIds(chainId);
            return elementRepository.ToLibiadaAlphabet(elements);
        }

        public List<long> GetChainElementIds(long chainId)
        {
            const string query = "SELECT unnest(alphabet) FROM chain WHERE id = @id";
            return db.ExecuteStoreQuery<long>(query, new NpgsqlParameter("@id", chainId)).ToList();
        }

        public int[] GetChainBuilding(long chainId)
        {
            const string query = "SELECT unnest(building) FROM chain WHERE id = @id";
            return db.ExecuteStoreQuery<int>(query, new NpgsqlParameter("@id", chainId)).ToArray();
        }

        public BaseChain ToLBaseChain(long chainId)
        {
            return new BaseChain(GetChainBuilding(chainId), GetChainAlphabet(chainId));
        }

        public Chain ToLibiadaChain(long chainId)
        {
            return new Chain(GetChainBuilding(chainId), GetChainAlphabet(chainId));
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}