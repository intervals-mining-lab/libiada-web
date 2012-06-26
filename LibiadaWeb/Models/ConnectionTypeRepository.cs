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
    public class ConnectionTypeRepository : IConnectionTypeRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<connection_type> All
        {
            get { return context.connection_type; }
        }

        public IQueryable<connection_type> AllIncluding(params Expression<Func<connection_type, object>>[] includeProperties)
        {
            IQueryable<connection_type> query = context.connection_type;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public connection_type Find(int id)
        {
            return context.connection_type.Single(x => x.id == id);
        }

        public void InsertOrUpdate(connection_type connection_type)
        {
            if (connection_type.id == default(int)) {
                // New entity
                context.connection_type.AddObject(connection_type);
            } else {
                // Existing entity
                context.connection_type.Attach(connection_type);
                context.ObjectStateManager.ChangeObjectState(connection_type, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var connection_type = context.connection_type.Single(x => x.id == id);
            context.connection_type.DeleteObject(connection_type);
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