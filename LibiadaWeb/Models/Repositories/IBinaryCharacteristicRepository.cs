using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories
{
    public interface IBinaryCharacteristicRepository : IDisposable
    {
        IQueryable<binary_characteristic> All { get; }
        IQueryable<binary_characteristic> AllIncluding(params Expression<Func<binary_characteristic, object>>[] includeProperties);
        binary_characteristic Find(long id);
        void InsertOrUpdate(binary_characteristic binary_characteristic);
        void Delete(long id);
        void Save();
    }
}
