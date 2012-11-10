using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface IMusicChainRepository : IDisposable
    {
        IQueryable<music_chain> All { get; }
        IQueryable<music_chain> AllIncluding(params Expression<Func<music_chain, object>>[] includeProperties);
        music_chain Find(long id);
        void InsertOrUpdate(music_chain music_chain);
        void Delete(long id);
        void Save();
    }
}