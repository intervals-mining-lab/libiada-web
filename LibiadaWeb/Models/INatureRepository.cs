using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface INatureRepository : IDisposable
    {
        IQueryable<nature> All { get; }
        IQueryable<nature> AllIncluding(params Expression<Func<nature, object>>[] includeProperties);
        nature Find(int id);
        void InsertOrUpdate(nature nature);
        void Delete(int id);
        void Save();
    }
}
