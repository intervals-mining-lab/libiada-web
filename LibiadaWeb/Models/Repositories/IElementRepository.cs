using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories
{
    public interface IElementRepository : IDisposable
    {
        IQueryable<element> All { get; }
        IQueryable<element> AllIncluding(params Expression<Func<element, object>>[] includeProperties);
        element Find(long id);
        void InsertOrUpdate(element element);
        void Delete(long id);
        void Save();
    }
}
