using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public interface ILinkUpRepository : IDisposable
    {
        IQueryable<link_up> All { get; }
        IQueryable<link_up> AllIncluding(params Expression<Func<link_up, object>>[] includeProperties);
        link_up Find(int id);
        void InsertOrUpdate(link_up link_up);
        void Delete(int id);
        void Save();
    }
}
