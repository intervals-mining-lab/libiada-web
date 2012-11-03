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
    public class NatureRepository : INatureRepository
    {
        LibiadaWebEntities db = new LibiadaWebEntities();

        public NatureRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<nature> All
        {
            get { return db.nature; }
        }

        public IQueryable<nature> AllIncluding(params Expression<Func<nature, object>>[] includeProperties)
        {
            IQueryable<nature> query = db.nature;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public nature Find(int id)
        {
            return db.nature.Single(x => x.id == id);
        }

        public void InsertOrUpdate(nature nature)
        {
            if (nature.id == default(int)) {
                // New entity
                db.nature.AddObject(nature);
            } else {
                // Existing entity
                db.nature.Attach(nature);
                db.ObjectStateManager.ChangeObjectState(nature, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var nature = db.nature.Single(x => x.id == id);
            db.nature.DeleteObject(nature);
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