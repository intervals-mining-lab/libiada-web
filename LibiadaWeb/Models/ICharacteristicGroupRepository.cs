using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface ICharacteristicGroupRepository : IDisposable
    {
        IQueryable<characteristic_group> All { get; }
        IQueryable<characteristic_group> AllIncluding(params Expression<Func<characteristic_group, object>>[] includeProperties);
        characteristic_group Find(int id);
        void InsertOrUpdate(characteristic_group characteristic_group);
        void Delete(int id);
        void Save();
    }
}
