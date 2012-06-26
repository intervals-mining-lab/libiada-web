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
    public class BuildingTypeRepository : IBuildingTypeRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<building_type> All
        {
            get { return context.building_type; }
        }

        public IQueryable<building_type> AllIncluding(params Expression<Func<building_type, object>>[] includeProperties)
        {
            IQueryable<building_type> query = context.building_type;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public building_type Find(int id)
        {
            return context.building_type.Single(x => x.id == id);
        }

        public void InsertOrUpdate(building_type building_type)
        {
            if (building_type.id == default(int)) {
                // New entity
                context.building_type.AddObject(building_type);
            } else {
                // Existing entity
                context.building_type.Attach(building_type);
                context.ObjectStateManager.ChangeObjectState(building_type, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var building_type = context.building_type.Single(x => x.id == id);
            context.building_type.DeleteObject(building_type);
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