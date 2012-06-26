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
    public class CharacteristicRepository : ICharacteristicRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<characteristic> All
        {
            get { return context.characteristic; }
        }

        public IQueryable<characteristic> AllIncluding(params Expression<Func<characteristic, object>>[] includeProperties)
        {
            IQueryable<characteristic> query = context.characteristic;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public characteristic Find(long id)
        {
            return context.characteristic.Single(x => x.id == id);
        }

        public void InsertOrUpdate(characteristic characteristic)
        {
            if (characteristic.id == default(long)) {
                // New entity
                context.characteristic.AddObject(characteristic);
            } else {
                // Existing entity
                context.characteristic.Attach(characteristic);
                context.ObjectStateManager.ChangeObjectState(characteristic, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var characteristic = context.characteristic.Single(x => x.id == id);
            context.characteristic.DeleteObject(characteristic);
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