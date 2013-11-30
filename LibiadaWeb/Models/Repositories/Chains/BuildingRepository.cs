using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly LibiadaWebEntities db;

        public BuildingRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<building> All {
            get { return db.building; }
        }
        public IQueryable<building> AllIncluding(params Expression<Func<building, object>>[] includeProperties)
        {
            IQueryable<building> query = db.building;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public void InsertOrUpdate(building building)
        {
            throw new NotImplementedException();
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