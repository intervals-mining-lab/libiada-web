using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{ 
    public class ConnectionRepository : IConnectionRepository
    {
        LibiadaWebEntities db = new LibiadaWebEntities();

        public ConnectionRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<connection> All
        {
            get { return db.connection; }
        }

        public IQueryable<connection> AllIncluding(params Expression<Func<connection, object>>[] includeProperties)
        {
            IQueryable<connection> query = db.connection;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public connection Find(long id)
        {
            return db.connection.Single(x => x.id == id);
        }

        public void InsertOrUpdate(connection connection)
        {
            if (connection.id == default(long)) {
                // New entity
                db.connection.AddObject(connection);
            } else {
                // Existing entity
                db.connection.Attach(connection);
                db.ObjectStateManager.ChangeObjectState(connection, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var connection = db.connection.Single(x => x.id == id);
            db.connection.DeleteObject(connection);
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