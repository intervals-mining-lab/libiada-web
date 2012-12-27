using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories
{ 
    public class HomogeneousCharacteristicRepository : IHomogeneousCharacteristicRepository
    {
        private readonly LibiadaWebEntities db;

        public HomogeneousCharacteristicRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<homogeneous_characteristic> All
        {
            get { return db.homogeneous_characteristic; }
        }

        public IQueryable<homogeneous_characteristic> AllIncluding(params Expression<Func<homogeneous_characteristic, object>>[] includeProperties)
        {
            IQueryable<homogeneous_characteristic> query = db.homogeneous_characteristic;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public homogeneous_characteristic Find(long id)
        {
            return db.homogeneous_characteristic.Single(x => x.id == id);
        }

        public void InsertOrUpdate(homogeneous_characteristic homogeneous_characteristic)
        {
            if (homogeneous_characteristic.id == default(long)) {
                // New entity
                db.homogeneous_characteristic.AddObject(homogeneous_characteristic);
            } else {
                // Existing entity
                db.homogeneous_characteristic.Attach(homogeneous_characteristic);
                db.ObjectStateManager.ChangeObjectState(homogeneous_characteristic, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var homogeneous_characteristic = Find(id);
            db.homogeneous_characteristic.DeleteObject(homogeneous_characteristic);
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