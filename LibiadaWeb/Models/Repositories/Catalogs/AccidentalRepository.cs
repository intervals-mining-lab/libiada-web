using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class AccidentalRepository : IAccidentalRepository
    {
        private readonly LibiadaWebEntities db;

        public AccidentalRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<accidental> All
        {
            get { return db.accidental; }
        }

        public IQueryable<accidental> AllIncluding(params Expression<Func<accidental, object>>[] includeProperties)
        {
            IQueryable<accidental> query = db.accidental;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public accidental Find(int id)
        {
            return db.accidental.Single(x => x.id == id);
        }

        public void InsertOrUpdate(accidental accidental)
        {
            if (accidental.id == default(int)) {
                // New entity
                db.accidental.AddObject(accidental);
            } else {
                // Existing entity
                db.accidental.Attach(accidental);
                db.ObjectStateManager.ChangeObjectState(accidental, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var accidental = Find(id);
            db.accidental.DeleteObject(accidental);
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