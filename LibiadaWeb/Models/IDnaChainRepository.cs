using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface IDnaChainRepository : IDisposable
    {
        IQueryable<dna_chain> All { get; }
        IQueryable<dna_chain> AllIncluding(params Expression<Func<dna_chain, object>>[] includeProperties);
        dna_chain Find(long id);
        void InsertOrUpdate(dna_chain dna_chain);
        void Delete(long id);
        void Save();
    }
}