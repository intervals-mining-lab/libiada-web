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
    public class RemoteDbRepository : IRemoteDbRepository
    {
        LibiadaWebEntities db = new LibiadaWebEntities();

        public RemoteDbRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<remote_db> All
        {
            get { return db.remote_db; }
        }

        public IQueryable<remote_db> AllIncluding(params Expression<Func<remote_db, object>>[] includeProperties)
        {
            IQueryable<remote_db> query = db.remote_db;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public remote_db Find(int id)
        {
            return db.remote_db.Single(x => x.id == id);
        }

        public void InsertOrUpdate(remote_db remote_db)
        {
            if (remote_db.id == default(int)) {
                // New entity
                db.remote_db.AddObject(remote_db);
            } else {
                // Existing entity
                db.remote_db.Attach(remote_db);
                db.ObjectStateManager.ChangeObjectState(remote_db, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var remote_db = db.remote_db.Single(x => x.id == id);
            db.remote_db.DeleteObject(remote_db);
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