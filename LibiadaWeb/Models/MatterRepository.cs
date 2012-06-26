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
    public class MatterRepository : IMatterRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<matter> All
        {
            get { return context.matter; }
        }

        public IQueryable<matter> AllIncluding(params Expression<Func<matter, object>>[] includeProperties)
        {
            IQueryable<matter> query = context.matter;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public matter Find(long id)
        {
            return context.matter.Single(x => x.id == id);
        }

        public void InsertOrUpdate(matter matter)
        {
            if (matter.id == default(long)) {
                // New entity
                context.matter.AddObject(matter);
            } else {
                // Existing entity
                context.matter.Attach(matter);
                context.ObjectStateManager.ChangeObjectState(matter, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var matter = context.matter.Single(x => x.id == id);
            context.matter.DeleteObject(matter);
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