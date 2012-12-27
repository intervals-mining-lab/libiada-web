using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{ 
    public class InstrumentRepository : IInstrumentRepository
    {
        private readonly LibiadaWebEntities db;

        public InstrumentRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<instrument> All
        {
            get { return db.instrument; }
        }

        public IQueryable<instrument> AllIncluding(params Expression<Func<instrument, object>>[] includeProperties)
        {
            IQueryable<instrument> query = db.instrument;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public instrument Find(int id)
        {
            return db.instrument.Single(x => x.id == id);
        }

        public void InsertOrUpdate(instrument instrument)
        {
            if (instrument.id == default(int)) {
                // New entity
                db.instrument.AddObject(instrument);
            } else {
                // Existing entity
                db.instrument.Attach(instrument);
                db.ObjectStateManager.ChangeObjectState(instrument, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var instrument = Find(id);
            db.instrument.DeleteObject(instrument);
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