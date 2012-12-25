using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using LibiadaWeb.Models.Repositories.Interfaces;

namespace LibiadaWeb.Models.Repositories
{ 
    public class CharacteristicGroupRepository : ICharacteristicGroupRepository
    {
        private readonly LibiadaWebEntities db;

        public CharacteristicGroupRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<characteristic_group> All
        {
            get { return db.characteristic_group; }
        }

        public IQueryable<characteristic_group> AllIncluding(params Expression<Func<characteristic_group, object>>[] includeProperties)
        {
            IQueryable<characteristic_group> query = db.characteristic_group;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public characteristic_group Find(int id)
        {
            return db.characteristic_group.Single(x => x.id == id);
        }

        public void InsertOrUpdate(characteristic_group characteristic_group)
        {
            if (characteristic_group.id == default(int)) {
                // New entity
                db.characteristic_group.AddObject(characteristic_group);
            } else {
                // Existing entity
                db.characteristic_group.Attach(characteristic_group);
                db.ObjectStateManager.ChangeObjectState(characteristic_group, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var characteristic_group = Find(id);
            db.characteristic_group.DeleteObject(characteristic_group);
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