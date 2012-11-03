using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{ 
    public class ConnectionTypeRepository : IConnectionTypeRepository
    {
        LibiadaWebEntities db = new LibiadaWebEntities();

        public ConnectionTypeRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<connection_type> All
        {
            get { return db.connection_type; }
        }

        public IQueryable<connection_type> AllIncluding(params Expression<Func<connection_type, object>>[] includeProperties)
        {
            IQueryable<connection_type> query = db.connection_type;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public connection_type Find(int id)
        {
            return db.connection_type.Single(x => x.id == id);
        }

        public void InsertOrUpdate(connection_type connection_type)
        {
            if (connection_type.id == default(int)) {
                // New entity
                db.connection_type.AddObject(connection_type);
            } else {
                // Existing entity
                db.connection_type.Attach(connection_type);
                db.ObjectStateManager.ChangeObjectState(connection_type, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var connection_type = db.connection_type.Single(x => x.id == id);
            db.connection_type.DeleteObject(connection_type);
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