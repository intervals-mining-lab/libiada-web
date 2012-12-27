using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public interface ILiteratureChainRepository : IDisposable
    {
        IQueryable<literature_chain> All { get; }
        IQueryable<literature_chain> AllIncluding(params Expression<Func<literature_chain, object>>[] includeProperties);
        literature_chain Find(long id);
        void InsertOrUpdate(literature_chain literature_chain);
        void Delete(long id);
        void Save();
    }
}