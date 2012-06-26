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
    public class ConnectionRepository : IConnectionRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<connection> All
        {
            get { return context.connection; }
        }

        public IQueryable<connection> AllIncluding(params Expression<Func<connection, object>>[] includeProperties)
        {
            IQueryable<connection> query = context.connection;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public connection Find(long id)
        {
            return context.connection.Single(x => x.id == id);
        }

        public void InsertOrUpdate(connection connection)
        {
            if (connection.id == default(long)) {
                // New entity
                context.connection.AddObject(connection);
            } else {
                // Existing entity
                context.connection.Attach(connection);
                context.ObjectStateManager.ChangeObjectState(connection, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var connection = context.connection.Single(x => x.id == id);
            context.connection.DeleteObject(connection);
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