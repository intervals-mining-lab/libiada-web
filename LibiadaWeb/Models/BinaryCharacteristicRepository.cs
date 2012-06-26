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
    public class BinaryCharacteristicRepository : IBinaryCharacteristicRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<binary_characteristic> All
        {
            get { return context.binary_characteristic; }
        }

        public IQueryable<binary_characteristic> AllIncluding(params Expression<Func<binary_characteristic, object>>[] includeProperties)
        {
            IQueryable<binary_characteristic> query = context.binary_characteristic;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public binary_characteristic Find(long id)
        {
            return context.binary_characteristic.Single(x => x.id == id);
        }

        public void InsertOrUpdate(binary_characteristic binary_characteristic)
        {
            if (binary_characteristic.id == default(long)) {
                // New entity
                context.binary_characteristic.AddObject(binary_characteristic);
            } else {
                // Existing entity
                context.binary_characteristic.Attach(binary_characteristic);
                context.ObjectStateManager.ChangeObjectState(binary_characteristic, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var binary_characteristic = context.binary_characteristic.Single(x => x.id == id);
            context.binary_characteristic.DeleteObject(binary_characteristic);
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