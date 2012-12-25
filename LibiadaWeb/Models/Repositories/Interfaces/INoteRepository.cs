using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface INoteRepository : IDisposable
    {
        IQueryable<note> All { get; }
        IQueryable<note> AllIncluding(params Expression<Func<note, object>>[] includeProperties);
        note Find(long id);
        void InsertOrUpdate(note note);
        void Delete(long id);
        void Save();
    }
}