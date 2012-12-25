using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface IChainRepository : IDisposable
    {
        IQueryable<chain> All { get; }
        IQueryable<chain> AllIncluding(params Expression<Func<chain, object>>[] includeProperties);
        chain Find(long id);
        void InsertOrUpdate(chain chain);
        void Delete(long id);
        void Save();
    }
}
