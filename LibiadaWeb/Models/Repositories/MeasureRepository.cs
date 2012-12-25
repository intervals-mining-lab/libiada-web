using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using LibiadaWeb.Models.Repositories.Interfaces;

namespace LibiadaWeb.Models.Repositories
{ 
    public class MeasureRepository : IMeasureRepository
    {
        private readonly LibiadaWebEntities db;

        public MeasureRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<measure> All
        {
            get { return db.measure; }
        }

        public IQueryable<measure> AllIncluding(params Expression<Func<measure, object>>[] includeProperties)
        {
            IQueryable<measure> query = db.measure;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public measure Find(long id)
        {
            return db.measure.Single(x => x.id == id);
        }

        public void InsertOrUpdate(measure measure)
        {
            if (measure.id == default(long)) {
                // New entity
                db.measure.AddObject(measure);
            } else {
                // Existing entity
                db.measure.Attach(measure);
                db.ObjectStateManager.ChangeObjectState(measure, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var measure = Find(id);
            db.measure.DeleteObject(measure);
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