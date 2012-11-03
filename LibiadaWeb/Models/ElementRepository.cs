using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{ 
    public class ElementRepository : IElementRepository
    {
        LibiadaWebEntities db = new LibiadaWebEntities();

        public ElementRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<element> All
        {
            get { return db.element; }
        }

        public IQueryable<element> AllIncluding(params Expression<Func<element, object>>[] includeProperties)
        {
            IQueryable<element> query = db.element;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public element Find(long id)
        {
            return db.element.Single(x => x.id == id);
        }

        public void InsertOrUpdate(element element)
        {
            if (element.id == default(long)) {
                // New entity
                db.element.AddObject(element);
            } else {
                // Existing entity
                db.element.Attach(element);
                db.ObjectStateManager.ChangeObjectState(element, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var element = db.element.Single(x => x.id == id);
            db.element.DeleteObject(element);
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