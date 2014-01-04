using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public interface IPieceTypeRepository : IDisposable
    {
        IQueryable<piece_type> All { get; }
        IQueryable<piece_type> AllIncluding(params Expression<Func<piece_type, object>>[] includeProperties);
        piece_type Find(int id);
        void InsertOrUpdate(piece_type piece_type);
        void Delete(int id);
        void Save();
    }
}