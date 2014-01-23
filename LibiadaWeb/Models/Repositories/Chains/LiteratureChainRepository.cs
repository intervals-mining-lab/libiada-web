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
    public class LiteratureChainRepository : ChainImporter, ILiteratureChainRepository
    {
        public LiteratureChainRepository(LibiadaWebEntities db) : base(db)
        {
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
            var parameters = FillParams(chain, alphabet, building);
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "@original",
                NpgsqlDbType = NpgsqlDbType.Boolean,
                Value = original
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "@language_id",
                NpgsqlDbType = NpgsqlDbType.Integer,
                Value = languageId
            });

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
                                        remote_db_id,
                                        original,
                                        language_id
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
                                        @language_id
                                    );";
            db.ExecuteStoreCommand(query, parameters.ToArray());
        }


        public void Insert(literature_chain chain, long[] alphabet, int[] building)
        {
            var literatureChain = new chain
            {
                id = chain.id,
                dissimilar = chain.dissimilar,
                notation_id = chain.notation_id,
                matter_id = chain.matter_id,
                piece_type_id = chain.piece_type_id,
                created = DateTime.Now,
                piece_position = chain.piece_position
            };

            Insert(literatureChain, chain.original, chain.language_id, alphabet, building);
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