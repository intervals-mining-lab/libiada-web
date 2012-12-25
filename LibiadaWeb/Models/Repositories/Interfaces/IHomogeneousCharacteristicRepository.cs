using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface IHomogeneousCharacteristicRepository : IDisposable
    {
        IQueryable<homogeneous_characteristic> All { get; }
        IQueryable<homogeneous_characteristic> AllIncluding(params Expression<Func<homogeneous_characteristic, object>>[] includeProperties);
        homogeneous_characteristic Find(long id);
        void InsertOrUpdate(homogeneous_characteristic homogeneous_characteristic);
        void Delete(long id);
        void Save();
    }
}