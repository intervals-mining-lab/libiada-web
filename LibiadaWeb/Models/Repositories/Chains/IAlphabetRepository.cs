using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public interface IAlphabetRepository : IDisposable
    {
        IQueryable<alphabet> All { get; }
        IQueryable<alphabet> AllIncluding(params Expression<Func<alphabet, object>>[] includeProperties);
        alphabet Find(long id);
        void InsertOrUpdate(alphabet alphabet);
        void Delete(long id);
        void Save();
    }
}