using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public class FmotivTypeRepository : IFmotivTypeRepository
    {
        private readonly LibiadaWebEntities db;

        public FmotivTypeRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<fmotiv_type> All
        {
            get { return db.fmotiv_type; }
        }

        public IQueryable<fmotiv_type> AllIncluding(params Expression<Func<fmotiv_type, object>>[] includeProperties)
        {
            IQueryable<fmotiv_type> query = db.fmotiv_type;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public fmotiv_type Find(int id)
        {
            return db.fmotiv_type.Single(x => x.id == id);
        }

        public void InsertOrUpdate(fmotiv_type fmotiv_type)
        {
            if (fmotiv_type.id == default(int)) {
                // New entity
                db.fmotiv_type.AddObject(fmotiv_type);
            } else {
                // Existing entity
                db.fmotiv_type.Attach(fmotiv_type);
                db.ObjectStateManager.ChangeObjectState(fmotiv_type, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var fmotiv_type = Find(id);
            db.fmotiv_type.DeleteObject(fmotiv_type);
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