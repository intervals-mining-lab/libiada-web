using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{
    public interface INoteSymbolRepository : IDisposable
    {
        IQueryable<note_symbol> All { get; }
        IQueryable<note_symbol> AllIncluding(params Expression<Func<note_symbol, object>>[] includeProperties);
        note_symbol Find(int id);
        void InsertOrUpdate(note_symbol note_symbol);
        void Delete(int id);
        void Save();
    }
}