using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories
{
    public interface ICongenericCharacteristicRepository : IDisposable
    {
        IQueryable<congeneric_characteristic> All { get; }
        IQueryable<congeneric_characteristic> AllIncluding(params Expression<Func<congeneric_characteristic, object>>[] includeProperties);
        congeneric_characteristic Find(long id);
        void InsertOrUpdate(congeneric_characteristic congeneric_characteristic);
        void Delete(long id);
        void Save();
    }
}