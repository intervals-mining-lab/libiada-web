using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface ICharacteristicRepository : IDisposable
    {
        IQueryable<characteristic> All { get; }
        IQueryable<characteristic> AllIncluding(params Expression<Func<characteristic, object>>[] includeProperties);
        characteristic Find(long id);
        void InsertOrUpdate(characteristic characteristic);
        void Delete(long id);
        void Save();
    }
}
