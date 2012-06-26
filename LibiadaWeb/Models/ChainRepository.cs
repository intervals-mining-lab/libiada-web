using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using LibiadaWeb;

namespace LibiadaWeb.Models
{ 
    public class ChainRepository : IChainRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<chain> All
        {
            get { return context.chain; }
        }

        public IQueryable<chain> AllIncluding(params Expression<Func<chain, object>>[] includeProperties)
        {
            IQueryable<chain> query = context.chain;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public chain Find(long id)
        {
            return context.chain.Single(x => x.id == id);
        }

        public void InsertOrUpdate(chain chain)
        {
            if (chain.id == default(long)) {
                // New entity
                context.chain.AddObject(chain);
            } else {
                // Existing entity
                context.chain.Attach(chain);
                context.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var chain = context.chain.Single(x => x.id == id);
            context.chain.DeleteObject(chain);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        // заполняет список чекбоксов цепочек
        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> chains)
        {
            HashSet<long> chainIds;
            if (chains != null)
            {
                chainIds = new HashSet<long>(chains.Select(c => c.id));
            }
            else
            {
                chainIds = new HashSet<long>();
            }
            var allChains = context.chain.Include("matter");
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
            context.Dispose();
        }
    }
}