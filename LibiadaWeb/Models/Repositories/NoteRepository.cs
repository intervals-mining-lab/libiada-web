using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using LibiadaWeb.Models.Repositories.Interfaces;

namespace LibiadaWeb.Models.Repositories
{ 
    public class NoteRepository : INoteRepository
    {
        private readonly LibiadaWebEntities db;

        public NoteRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<note> All
        {
            get { return db.note; }
        }

        public IQueryable<note> AllIncluding(params Expression<Func<note, object>>[] includeProperties)
        {
            IQueryable<note> query = db.note;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public note Find(long id)
        {
            return db.note.Single(x => x.id == id);
        }

        public void InsertOrUpdate(note note)
        {
            if (note.id == default(long)) {
                // New entity
                db.note.AddObject(note);
            } else {
                // Existing entity
                db.note.Attach(note);
                db.ObjectStateManager.ChangeObjectState(note, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var note = Find(id);
            db.note.DeleteObject(note);
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