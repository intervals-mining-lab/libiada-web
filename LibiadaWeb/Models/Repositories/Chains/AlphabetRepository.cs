using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class AlphabetRepository : IAlphabetRepository
    {
        private readonly LibiadaWebEntities db;

        public AlphabetRepository(LibiadaWebEntities db)
        {
            this.db = db;
            new ElementRepository(db);
        }

        public IQueryable<alphabet> All
        {
            get { return db.alphabet; }
        }

        public IQueryable<alphabet> AllIncluding(params Expression<Func<alphabet, object>>[] includeProperties)
        {
            IQueryable<alphabet> query = db.alphabet;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public alphabet Find(long id)
        {
            return db.alphabet.Single(x => x.id == id);
        }

        public void InsertOrUpdate(alphabet alphabet)
        {
            if (alphabet.id == default(long))
            {
                // New entity
                db.alphabet.AddObject(alphabet);
            }
            else
            {
                // Existing entity
                db.alphabet.Attach(alphabet);
                db.ObjectStateManager.ChangeObjectState(alphabet, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var alphabet = Find(id);
            db.alphabet.DeleteObject(alphabet);
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