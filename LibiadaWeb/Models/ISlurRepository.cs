using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface ISlurRepository : IDisposable
    {
        IQueryable<slur> All { get; }
        IQueryable<slur> AllIncluding(params Expression<Func<slur, object>>[] includeProperties);
        slur Find(int id);
        void InsertOrUpdate(slur slur);
        void Delete(int id);
        void Save();
    }
}