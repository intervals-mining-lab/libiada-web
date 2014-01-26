using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{ 
    public class RemoteDbRepository : IRemoteDbRepository
    {
        private readonly LibiadaWebEntities db;

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

        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = false,
                Nature = n.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(int selectedDb)
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = n.id == selectedDb,
                Nature = n.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(List<int> selectedDbs)
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = selectedDbs.Contains(n.id),
                Nature = n.nature_id
            });
        }

        public void Delete(int id)
        {
            var remote_db = Find(id);
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