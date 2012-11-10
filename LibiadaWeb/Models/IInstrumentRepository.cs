using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface IInstrumentRepository : IDisposable
    {
        IQueryable<instrument> All { get; }
        IQueryable<instrument> AllIncluding(params Expression<Func<instrument, object>>[] includeProperties);
        instrument Find(int id);
        void InsertOrUpdate(instrument instrument);
        void Delete(int id);
        void Save();
    }
}