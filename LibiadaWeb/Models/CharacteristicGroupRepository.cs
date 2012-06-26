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
    public class CharacteristicGroupRepository : ICharacteristicGroupRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<characteristic_group> All
        {
            get { return context.characteristic_group; }
        }

        public IQueryable<characteristic_group> AllIncluding(params Expression<Func<characteristic_group, object>>[] includeProperties)
        {
            IQueryable<characteristic_group> query = context.characteristic_group;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public characteristic_group Find(int id)
        {
            return context.characteristic_group.Single(x => x.id == id);
        }

        public void InsertOrUpdate(characteristic_group characteristic_group)
        {
            if (characteristic_group.id == default(int)) {
                // New entity
                context.characteristic_group.AddObject(characteristic_group);
            } else {
                // Existing entity
                context.characteristic_group.Attach(characteristic_group);
                context.ObjectStateManager.ChangeObjectState(characteristic_group, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var characteristic_group = context.characteristic_group.Single(x => x.id == id);
            context.characteristic_group.DeleteObject(characteristic_group);
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