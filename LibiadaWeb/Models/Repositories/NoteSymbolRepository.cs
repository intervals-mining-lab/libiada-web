using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using LibiadaWeb.Models.Repositories.Interfaces;

namespace LibiadaWeb.Models.Repositories
{ 
    public class NoteSymbolRepository : INoteSymbolRepository
    {
        private readonly LibiadaWebEntities db;

        public NoteSymbolRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<note_symbol> All
        {
            get { return db.note_symbol; }
        }

        public IQueryable<note_symbol> AllIncluding(params Expression<Func<note_symbol, object>>[] includeProperties)
        {
            IQueryable<note_symbol> query = db.note_symbol;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public note_symbol Find(int id)
        {
            return db.note_symbol.Single(x => x.id == id);
        }

        public void InsertOrUpdate(note_symbol note_symbol)
        {
            if (note_symbol.id == default(int)) {
                // New entity
                db.note_symbol.AddObject(note_symbol);
            } else {
                // Existing entity
                db.note_symbol.Attach(note_symbol);
                db.ObjectStateManager.ChangeObjectState(note_symbol, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var note_symbol = Find(id);
            db.note_symbol.DeleteObject(note_symbol);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose() 
        {
            db.Dispose();
        }
    }
}