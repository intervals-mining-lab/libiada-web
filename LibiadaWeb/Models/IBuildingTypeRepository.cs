using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface IBuildingTypeRepository : IDisposable
    {
        IQueryable<building_type> All { get; }
        IQueryable<building_type> AllIncluding(params Expression<Func<building_type, object>>[] includeProperties);
        building_type Find(int id);
        void InsertOrUpdate(building_type building_type);
        void Delete(int id);
        void Save();
    }
}
