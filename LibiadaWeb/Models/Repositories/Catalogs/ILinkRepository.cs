using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public interface ILinkRepository : IDisposable
    {
        IQueryable<link> All { get; }
        IQueryable<link> AllIncluding(params Expression<Func<link, object>>[] includeProperties);
        link Find(int id);
        void InsertOrUpdate(link link);
        void Delete(int id);
        void Save();
    }
}
