using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public interface IBuildingRepository : IDisposable
    {
        IQueryable<building> All { get; }
        IQueryable<building> AllIncluding(params Expression<Func<building, object>>[] includeProperties);
        void InsertOrUpdate(building building);
        void Save(); 
    }
}