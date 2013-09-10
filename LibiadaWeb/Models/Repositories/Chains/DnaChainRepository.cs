using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class DnaChainRepository : IDnaChainRepository
    {
        private readonly LibiadaWebEntities db;
        private readonly BuildingRepository buildingRepository;

        public DnaChainRepository(LibiadaWebEntities db)
        {
            this.db = db;
            buildingRepository = new BuildingRepository(db);
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

        public void InsertOrUpdate(dna_chain chain)
        {
            if (chain.id == default(long))
            {
                // New entity
                db.dna_chain.AddObject(chain);
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
            HashSet<long> chainIds = chains != null
                                         ? new HashSet<long>(chains.Select(c => c.id))
                                         : new HashSet<long>();
            var allChains = db.dna_chain.Include("matter");
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