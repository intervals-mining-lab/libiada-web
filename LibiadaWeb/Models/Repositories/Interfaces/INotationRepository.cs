using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface INotationRepository : IDisposable
    {
        IQueryable<notation> All { get; }
        IQueryable<notation> AllIncluding(params Expression<Func<notation, object>>[] includeProperties);
        notation Find(int id);
        void InsertOrUpdate(notation notation);
        void Delete(int id);
        void Save();
    }
}
