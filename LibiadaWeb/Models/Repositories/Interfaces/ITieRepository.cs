using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface ITieRepository : IDisposable
    {
        IQueryable<tie> All { get; }
        IQueryable<tie> AllIncluding(params Expression<Func<tie, object>>[] includeProperties);
        tie Find(int id);
        void InsertOrUpdate(tie tie);
        void Delete(int id);
        void Save();
    }
}