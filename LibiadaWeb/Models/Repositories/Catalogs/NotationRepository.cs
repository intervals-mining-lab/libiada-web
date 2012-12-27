using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories.Catalogs
{ 
    public class NotationRepository : INotationRepository
    {
        private readonly LibiadaWebEntities db;

        public NotationRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<notation> All
        {
            get { return db.notation; }
        }

        public IQueryable<notation> AllIncluding(params Expression<Func<notation, object>>[] includeProperties)
        {
            IQueryable<notation> query = db.notation;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public notation Find(int id)
        {
            return db.notation.Single(x => x.id == id);
        }

        public void InsertOrUpdate(notation notation)
        {
            if (notation.id == default(int)) {
                // New entity
                db.notation.AddObject(notation);
            } else {
                // Existing entity
                db.notation.Attach(notation);
                db.ObjectStateManager.ChangeObjectState(notation, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var notation = Find(id);
            db.notation.DeleteObject(notation);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<notation> notations)
        {
            HashSet<int> notationIds;
            if (notations != null)
            {
                notationIds = new HashSet<int>(notations.Select(c => c.id));
            }
            else
            {
                notationIds = new HashSet<int>();
            }
            var allNotations = db.notation;
            var notationsList = new List<SelectListItem>();
            foreach (var notation in allNotations)
            {
                notationsList.Add(new SelectListItem
                {
                    Value = notation.id.ToString(),
                    Text = notation.name,
                    Selected = notationIds.Contains(notation.id)
                });
            }
            return notationsList;
        }

        public void Dispose() 
        {
            db.Dispose();
        }
    }
}