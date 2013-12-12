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
    public class DnaChainRepository : ChainImporter, IDnaChainRepository
    {
        public DnaChainRepository(LibiadaWebEntities db) : base(db)
        {
        }

        public IQueryable<dna_chain> All
        {
            get { return db.dna_chain; }
        }

        public IQueryable<dna_chain> AllIncluding(params Expression<Func<dna_chain, object>>[] includeProperties)
        {
            IQueryable<dna_chain> query = db.dna_chain;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public dna_chain Find(long id)
        {
            return db.dna_chain.Single(x => x.id == id);
        }

        public void Insert(chain chain, string fastaHeader, long[] alphabet, int[] building)
        {
            var parameters = FillParams(chain, alphabet, building);
            parameters.Add(new NpgsqlParameter
                {
                    ParameterName = "@fasta_header",
                    NpgsqlDbType = NpgsqlDbType.Varchar,
                    Value = fastaHeader
                });

            String query = @"SELECT create_dna_chain(
                                    @id,
                                    @notation_id,
                                    @matter_id,
                                    @piece_type_id,
                                    @fasta_header,
                                    @alphabet,
                                    @building,
                                    @remote_id,
                                    @remote_db_id,
                                    @creation_date,
                                    @piece_position,
                                    @dissimilar);";
            db.ExecuteStoreCommand(query, parameters.ToArray());

        }


        public void Insert(dna_chain chain, long[] alphabet, int[] building)
        {
            var dnaChain = new chain
            {
                id = chain.id,
                dissimilar = chain.dissimilar,
                notation_id = chain.notation_id,
                matter_id = chain.matter_id,
                piece_type_id = chain.piece_type_id,
                creation_date = DateTime.Now,
                piece_position = chain.piece_position
            };

            

            Insert(dnaChain, chain.fasta_header, alphabet, building);
        }

        public void InsertOrUpdate(dna_chain chain)
        {
            if (chain.id == default(long))
            {
                // New entity
                throw new NotSupportedException("Для добавления новых записей следует использовать метод Insert.");
            }
            else
            {
                // Existing entity
                db.dna_chain.Attach(chain);
                db.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var chain = Find(id);
            db.dna_chain.DeleteObject(chain);
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

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}