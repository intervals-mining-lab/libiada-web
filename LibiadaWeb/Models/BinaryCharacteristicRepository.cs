using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{ 
    public class BinaryCharacteristicRepository : IBinaryCharacteristicRepository
    {
        LibiadaWebEntities db;

        public BinaryCharacteristicRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<binary_characteristic> All
        {
            get { return db.binary_characteristic; }
        }

        public IQueryable<binary_characteristic> AllIncluding(params Expression<Func<binary_characteristic, object>>[] includeProperties)
        {
            IQueryable<binary_characteristic> query = db.binary_characteristic;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public binary_characteristic Find(long id)
        {
            return db.binary_characteristic.Single(x => x.id == id);
        }

        public void InsertOrUpdate(binary_characteristic binary_characteristic)
        {
            if (binary_characteristic.id == default(long)) {
                // New entity
                db.binary_characteristic.AddObject(binary_characteristic);
            } else {
                // Existing entity
                db.binary_characteristic.Attach(binary_characteristic);
                db.ObjectStateManager.ChangeObjectState(binary_characteristic, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var binary_characteristic = db.binary_characteristic.Single(x => x.id == id);
            db.binary_characteristic.DeleteObject(binary_characteristic);
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