using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface IPitchRepository : IDisposable
    {
        IQueryable<pitch> All { get; }
        IQueryable<pitch> AllIncluding(params Expression<Func<pitch, object>>[] includeProperties);
        pitch Find(int id);
        void InsertOrUpdate(pitch pitch);
        void Delete(int id);
        void Save();
    }
}