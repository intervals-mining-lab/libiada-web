using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface IAccidentalRepository : IDisposable
    {
        IQueryable<accidental> All { get; }
        IQueryable<accidental> AllIncluding(params Expression<Func<accidental, object>>[] includeProperties);
        accidental Find(int id);
        void InsertOrUpdate(accidental accidental);
        void Delete(int id);
        void Save();
    }
}