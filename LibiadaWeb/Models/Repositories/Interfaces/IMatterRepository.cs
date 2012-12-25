using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface IMatterRepository : IDisposable
    {
        IQueryable<matter> All { get; }
        IQueryable<matter> AllIncluding(params Expression<Func<matter, object>>[] includeProperties);
        matter Find(long id);
        void InsertOrUpdate(matter matter);
        void Delete(long id);
        void Save();
    }
}
