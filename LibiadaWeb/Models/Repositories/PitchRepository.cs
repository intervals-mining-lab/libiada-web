using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories
{ 
    public class PitchRepository : IPitchRepository
    {
        private readonly LibiadaWebEntities db;

        public PitchRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<pitch> All
        {
            get { return db.pitch; }
        }

        public IQueryable<pitch> AllIncluding(params Expression<Func<pitch, object>>[] includeProperties)
        {
            IQueryable<pitch> query = db.pitch;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public pitch Find(int id)
        {
            return db.pitch.Single(x => x.id == id);
        }

        public void InsertOrUpdate(pitch pitch)
        {
            if (pitch.id == default(int)) {
                // New entity
                db.pitch.AddObject(pitch);
            } else {
                // Existing entity
                db.pitch.Attach(pitch);
                db.ObjectStateManager.ChangeObjectState(pitch, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var pitch = Find(id);
            db.pitch.DeleteObject(pitch);
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