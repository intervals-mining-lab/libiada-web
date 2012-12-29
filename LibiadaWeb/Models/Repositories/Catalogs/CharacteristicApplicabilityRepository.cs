using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{ 
    public class CharacteristicApplicabilityRepository : ICharacteristicApplicabilityRepository
    {
        private readonly LibiadaWebEntities db;

        public CharacteristicApplicabilityRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<characteristic_applicability> All
        {
            get { return db.characteristic_applicability; }
        }

        public IQueryable<characteristic_applicability> AllIncluding(params Expression<Func<characteristic_applicability, object>>[] includeProperties)
        {
            IQueryable<characteristic_applicability> query = db.characteristic_applicability;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public characteristic_applicability Find(int id)
        {
            return db.characteristic_applicability.Single(x => x.id == id);
        }

        public void InsertOrUpdate(characteristic_applicability characteristic_applicability)
        {
            if (characteristic_applicability.id == default(int)) {
                // New entity
                db.characteristic_applicability.AddObject(characteristic_applicability);
            } else {
                // Existing entity
                db.characteristic_applicability.Attach(characteristic_applicability);
                db.ObjectStateManager.ChangeObjectState(characteristic_applicability, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var characteristic_applicability = Find(id);
            db.characteristic_applicability.DeleteObject(characteristic_applicability);
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