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
            foreach (var includeProperty in includeProperties)
            {
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
            if (notation.id == default(int))
            {
                // New entity
                db.notation.AddObject(notation);
            }
            else
            {
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
            HashSet<int> notationIds = notations != null
                                           ? new HashSet<int>(notations.Select(c => c.id))
                                           : new HashSet<int>();
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

        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.notation.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = false,
                Nature = n.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(int selectedNotation)
        {
            return db.notation.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = n.id == selectedNotation,
                Nature = n.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(List<int> selectedNotations)
        {
            return db.notation.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = selectedNotations.Contains(n.id),
                Nature = n.nature_id
            });
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}