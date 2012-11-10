using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models
{ 
    public class MusicChainRepository : IMusicChainRepository
    {
        private readonly LibiadaWebEntities db;

        public MusicChainRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<music_chain> All
        {
            get { return db.music_chain; }
        }

        public IQueryable<music_chain> AllIncluding(params Expression<Func<music_chain, object>>[] includeProperties)
        {
            IQueryable<music_chain> query = db.music_chain;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public music_chain Find(long id)
        {
            return db.music_chain.Single(x => x.id == id);
        }

        public void InsertOrUpdate(music_chain music_chain)
        {
            if (music_chain.id == default(long)) {
                // New entity
                db.music_chain.AddObject(music_chain);
            } else {
                // Existing entity
                db.music_chain.Attach(music_chain);
                db.ObjectStateManager.ChangeObjectState(music_chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var music_chain = Find(id);
            db.music_chain.DeleteObject(music_chain);
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