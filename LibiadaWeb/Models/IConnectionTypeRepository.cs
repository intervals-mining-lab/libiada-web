using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface IConnectionTypeRepository : IDisposable
    {
        IQueryable<connection_type> All { get; }
        IQueryable<connection_type> AllIncluding(params Expression<Func<connection_type, object>>[] includeProperties);
        connection_type Find(int id);
        void InsertOrUpdate(connection_type connection_type);
        void Delete(int id);
        void Save();
    }
}
