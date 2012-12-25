using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using LibiadaWeb.Models.Repositories.Interfaces;

namespace LibiadaWeb.Models.Repositories
{ 
    public class TieRepository : ITieRepository
    {
        private readonly LibiadaWebEntities db;

        public TieRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<tie> All
        {
            get { return db.tie; }
        }

        public IQueryable<tie> AllIncluding(params Expression<Func<tie, object>>[] includeProperties)
        {
            IQueryable<tie> query = db.tie;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public tie Find(int id)
        {
            return db.tie.Single(x => x.id == id);
        }

        public void InsertOrUpdate(tie tie)
        {
            if (tie.id == default(int)) {
                // New entity
                db.tie.AddObject(tie);
            } else {
                // Existing entity
                db.tie.Attach(tie);
                db.ObjectStateManager.ChangeObjectState(tie, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var tie = Find(id);
            db.tie.DeleteObject(tie);
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