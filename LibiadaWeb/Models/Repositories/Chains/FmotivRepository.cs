using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Chains
{ 
    public class FmotivRepository : IFmotivRepository
    {
        private readonly LibiadaWebEntities db;

        public FmotivRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<fmotiv> All
        {
            get { return db.fmotiv; }
        }

        public IQueryable<fmotiv> AllIncluding(params Expression<Func<fmotiv, object>>[] includeProperties)
        {
            IQueryable<fmotiv> query = db.fmotiv;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public fmotiv Find(long id)
        {
            return db.fmotiv.Single(x => x.id == id);
        }

        public void InsertOrUpdate(fmotiv fmotiv)
        {
            if (fmotiv.id == default(long)) {
                // New entity
                db.fmotiv.AddObject(fmotiv);
            } else {
                // Existing entity
                db.fmotiv.Attach(fmotiv);
                db.ObjectStateManager.ChangeObjectState(fmotiv, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var fmotiv = Find(id);
            db.fmotiv.DeleteObject(fmotiv);
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