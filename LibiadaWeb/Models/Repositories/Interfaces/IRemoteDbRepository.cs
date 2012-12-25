using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface IRemoteDbRepository : IDisposable
    {
        IQueryable<remote_db> All { get; }
        IQueryable<remote_db> AllIncluding(params Expression<Func<remote_db, object>>[] includeProperties);
        remote_db Find(int id);
        void InsertOrUpdate(remote_db remote_db);
        void Delete(int id);
        void Save();
    }
}