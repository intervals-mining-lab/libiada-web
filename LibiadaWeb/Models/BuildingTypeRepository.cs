using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{ 
    public class BuildingTypeRepository : IBuildingTypeRepository
    {
        LibiadaWebEntities db = new LibiadaWebEntities();

        public BuildingTypeRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<building_type> All
        {
            get { return db.building_type; }
        }

        public IQueryable<building_type> AllIncluding(params Expression<Func<building_type, object>>[] includeProperties)
        {
            IQueryable<building_type> query = db.building_type;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public building_type Find(int id)
        {
            return db.building_type.Single(x => x.id == id);
        }

        public void InsertOrUpdate(building_type building_type)
        {
            if (building_type.id == default(int)) {
                // New entity
                db.building_type.AddObject(building_type);
            } else {
                // Existing entity
                db.building_type.Attach(building_type);
                db.ObjectStateManager.ChangeObjectState(building_type, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var building_type = db.building_type.Single(x => x.id == id);
            db.building_type.DeleteObject(building_type);
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