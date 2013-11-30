using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using LibiadaWeb.Helpers;
using Npgsql;
using NpgsqlTypes;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class LiteratureChainRepository : ILiteratureChainRepository
    {
        private readonly LibiadaWebEntities db;

        public LiteratureChainRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<literature_chain> All
        {
            get { return db.literature_chain; }
        }

        public IQueryable<literature_chain> AllIncluding(
            params Expression<Func<literature_chain, object>>[] includeProperties)
        {
            IQueryable<literature_chain> query = db.literature_chain;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public literature_chain Find(long id)
        {
            return db.literature_chain.Single(x => x.id == id);
        }

        public void Insert(chain chain, bool original, int languageId, long[] alphabet, int[] building)
        {
            literature_chain literatureChain = new literature_chain
            {
                id = chain.id,
                dissimilar = chain.dissimilar,
                notation_id = chain.notation_id,
                matter_id = chain.matter_id,
                original = original,
                language_id = languageId,
                piece_type_id = chain.piece_type_id,
                creation_date = DateTime.Now,
                piece_position = chain.piece_position
            };

            Insert(literatureChain, alphabet, building);
        }


        public void Insert(literature_chain chain, long[] alphabet, int[] building)
        {
            if (chain.id == 0)
            {
                chain.id = DataTransformators.GetLongSequenceValue(db, "chain_id_seq");
            }

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
                            ParameterName = "@original",
                            NpgsqlDbType = NpgsqlDbType.Boolean,
                            Value = chain.original
                        },
                    new NpgsqlParameter
                        {
                            ParameterName = "@language_id",
                            NpgsqlDbType = NpgsqlDbType.Integer,
                            Value = chain.language_id
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
                "SELECT create_literature_chain(@id,@notation_id,@matter_id,@piece_type_id,@original,@language_id,@alphabet,@building,@creation_date,@piece_position,@dissimilar);";
            db.ExecuteStoreCommand(query, parameters);
        }

        public void InsertOrUpdate(literature_chain literature_chain)
        {
            if (literature_chain.id == default(long))
            {
                // New entity
                throw new NotSupportedException("Для добавления новых записей следует использовать метод Insert.");
            }
            else
            {
                // Existing entity
                db.literature_chain.Attach(literature_chain);
                db.ObjectStateManager.ChangeObjectState(literature_chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var literature_chain = Find(id);
            db.literature_chain.DeleteObject(literature_chain);
        }

        public void Save()
        {
            db.SaveChanges();
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