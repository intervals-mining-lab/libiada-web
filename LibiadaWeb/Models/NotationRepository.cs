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
    public class NotationRepository : INotationRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<notation> All
        {
            get { return context.notation; }
        }

        public IQueryable<notation> AllIncluding(params Expression<Func<notation, object>>[] includeProperties)
        {
            IQueryable<notation> query = context.notation;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public notation Find(int id)
        {
            return context.notation.Single(x => x.id == id);
        }

        public void InsertOrUpdate(notation notation)
        {
            if (notation.id == default(int)) {
                // New entity
                context.notation.AddObject(notation);
            } else {
                // Existing entity
                context.notation.Attach(notation);
                context.ObjectStateManager.ChangeObjectState(notation, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var notation = context.notation.Single(x => x.id == id);
            context.notation.DeleteObject(notation);
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