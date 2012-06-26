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
    public class ElementRepository : IElementRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<element> All
        {
            get { return context.element; }
        }

        public IQueryable<element> AllIncluding(params Expression<Func<element, object>>[] includeProperties)
        {
            IQueryable<element> query = context.element;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public element Find(long id)
        {
            return context.element.Single(x => x.id == id);
        }

        public void InsertOrUpdate(element element)
        {
            if (element.id == default(long)) {
                // New entity
                context.element.AddObject(element);
            } else {
                // Existing entity
                context.element.Attach(element);
                context.ObjectStateManager.ChangeObjectState(element, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var element = context.element.Single(x => x.id == id);
            context.element.DeleteObject(element);
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