using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface IConnectionRepository : IDisposable
    {
        IQueryable<connection> All { get; }
        IQueryable<connection> AllIncluding(params Expression<Func<connection, object>>[] includeProperties);
        connection Find(long id);
        void InsertOrUpdate(connection connection);
        void Delete(long id);
        void Save();
    }
}
