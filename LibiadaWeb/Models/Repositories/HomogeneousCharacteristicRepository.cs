using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories
{ 
    public class CongenericCharacteristicRepository : ICongenericCharacteristicRepository
    {
        private readonly LibiadaWebEntities db;

        public CongenericCharacteristicRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<congeneric_characteristic> All
        {
            get { return db.congeneric_characteristic; }
        }

        public IQueryable<congeneric_characteristic> AllIncluding(params Expression<Func<congeneric_characteristic, object>>[] includeProperties)
        {
            IQueryable<congeneric_characteristic> query = db.congeneric_characteristic;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public congeneric_characteristic Find(long id)
        {
            return db.congeneric_characteristic.Single(x => x.id == id);
        }

        public void InsertOrUpdate(congeneric_characteristic congeneric_characteristic)
        {
            if (congeneric_characteristic.id == default(long)) {
                // New entity
                db.congeneric_characteristic.AddObject(congeneric_characteristic);
            } else {
                // Existing entity
                db.congeneric_characteristic.Attach(congeneric_characteristic);
                db.ObjectStateManager.ChangeObjectState(congeneric_characteristic, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var congeneric_characteristic = Find(id);
            db.congeneric_characteristic.DeleteObject(congeneric_characteristic);
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