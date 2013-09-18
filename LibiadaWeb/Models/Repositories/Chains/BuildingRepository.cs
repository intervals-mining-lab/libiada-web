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

        public int[] ToArray(long chainId)
        {
            String query = "SELECT number FROM building WHERE chain_id = " + chainId + " ORDER BY index";
            return db.ExecuteStoreQuery<int>(query).ToArray();
        }

        public int ToDbBuilding(long chainId, int[] libiadaBuilding)
        {
            String aggregatedBuilding = libiadaBuilding.Aggregate(new StringBuilder(), (a, b) =>
                                                                  a.Append("," + b.ToString()),
                                                                  a => a.Remove(0, 1).ToString());
            String query = "SELECT create_building_from_string(" + chainId + ", '" + aggregatedBuilding + "')";
            return db.ExecuteStoreQuery<int>(query).First();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}