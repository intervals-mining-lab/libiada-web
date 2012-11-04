using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{ 
    public class CharacteristicRepository : ICharacteristicRepository
    {
        private readonly LibiadaWebEntities db;

        public CharacteristicRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<characteristic> All
        {
            get { return db.characteristic; }
        }

        public IQueryable<characteristic> AllIncluding(params Expression<Func<characteristic, object>>[] includeProperties)
        {
            IQueryable<characteristic> query = db.characteristic;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public characteristic Find(long id)
        {
            return db.characteristic.Single(x => x.id == id);
        }

        public void InsertOrUpdate(characteristic characteristic)
        {
            if (characteristic.id == default(long)) {
                // New entity
                db.characteristic.AddObject(characteristic);
            } else {
                // Existing entity
                db.characteristic.Attach(characteristic);
                db.ObjectStateManager.ChangeObjectState(characteristic, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var characteristic = db.characteristic.Single(x => x.id == id);
            db.characteristic.DeleteObject(characteristic);
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