using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using LibiadaWeb;

namespace LibiadaWeb.Models
{ 
    public class NatureRepository : INatureRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<nature> All
        {
            get { return context.nature; }
        }

        public IQueryable<nature> AllIncluding(params Expression<Func<nature, object>>[] includeProperties)
        {
            IQueryable<nature> query = context.nature;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public nature Find(int id)
        {
            return context.nature.Single(x => x.id == id);
        }

        public void InsertOrUpdate(nature nature)
        {
            if (nature.id == default(int)) {
                // New entity
                context.nature.AddObject(nature);
            } else {
                // Existing entity
                context.nature.Attach(nature);
                context.ObjectStateManager.ChangeObjectState(nature, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var nature = context.nature.Single(x => x.id == id);
            context.nature.DeleteObject(nature);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void Dispose() 
        {
            context.Dispose();
        }
    }
}