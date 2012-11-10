using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{ 
    public class SlurRepository : ISlurRepository
    {
        private readonly LibiadaWebEntities db;

        public SlurRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<slur> All
        {
            get { return db.slur; }
        }

        public IQueryable<slur> AllIncluding(params Expression<Func<slur, object>>[] includeProperties)
        {
            IQueryable<slur> query = db.slur;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public slur Find(int id)
        {
            return db.slur.Single(x => x.id == id);
        }

        public void InsertOrUpdate(slur slur)
        {
            if (slur.id == default(int)) {
                // New entity
                db.slur.AddObject(slur);
            } else {
                // Existing entity
                db.slur.Attach(slur);
                db.ObjectStateManager.ChangeObjectState(slur, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var slur = Find(id);
            db.slur.DeleteObject(slur);
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