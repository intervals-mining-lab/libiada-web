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
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<remote_db> All
        {
            get { return context.remote_db; }
        }

        public IQueryable<remote_db> AllIncluding(params Expression<Func<remote_db, object>>[] includeProperties)
        {
            IQueryable<remote_db> query = context.remote_db;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public remote_db Find(int id)
        {
            return context.remote_db.Single(x => x.id == id);
        }

        public void InsertOrUpdate(remote_db remote_db)
        {
            if (remote_db.id == default(int)) {
                // New entity
                context.remote_db.AddObject(remote_db);
            } else {
                // Existing entity
                context.remote_db.Attach(remote_db);
                context.ObjectStateManager.ChangeObjectState(remote_db, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var remote_db = context.remote_db.Single(x => x.id == id);
            context.remote_db.DeleteObject(remote_db);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void Dispose() 
        {
            context.Dispose();
        }
    }
}