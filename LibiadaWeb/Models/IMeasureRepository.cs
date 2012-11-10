using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface IMeasureRepository : IDisposable
    {
        IQueryable<measure> All { get; }
        IQueryable<measure> AllIncluding(params Expression<Func<measure, object>>[] includeProperties);
        measure Find(long id);
        void InsertOrUpdate(measure measure);
        void Delete(long id);
        void Save();
    }
}