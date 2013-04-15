using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories
{ 
    public class ElementRepository : IElementRepository
    {
        private readonly LibiadaWebEntities db;

        public ElementRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<element> All
        {
            get { return db.element; }
        }

        public IQueryable<element> AllIncluding(params Expression<Func<element, object>>[] includeProperties)
        {
            IQueryable<element> query = db.element;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public element Find(long id)
        {
            return db.element.Single(x => x.id == id);
        }

        public void InsertOrUpdate(element element)
        {
            if (element.id == default(long)) {
                // New entity
                db.element.AddObject(element);
            } else {
                // Existing entity
                db.element.Attach(element);
                db.ObjectStateManager.ChangeObjectState(element, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var element = Find(id);
            db.element.DeleteObject(element);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose() 
        {
            db.Dispose();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<element> allElements, IEnumerable<element> selectedElement)
        {
            HashSet<long> elementIds;
            if (selectedElement != null)
            {
                elementIds = new HashSet<long>(selectedElement.Select(c => c.id));
            }
            else
            {
                elementIds = new HashSet<long>();
            }
            if (allElements == null)
            {
                allElements = db.element;
            }
            var elementsList = new List<SelectListItem>();
            foreach (var element in allElements)
            {
                elementsList.Add(new SelectListItem
                {
                    Value = element.id.ToString(),
                    Text = element.name,
                    Selected = elementIds.Contains(element.id)
                });
            }
            return elementsList;
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<element> elements)
        {
            HashSet<long> elementIds;
            if (elements != null)
            {
                elementIds = new HashSet<long>(elements.Select(c => c.id));
            }
            else
            {
                elementIds = new HashSet<long>();
            }
            var allElements = db.element;
            var elementsList = new List<SelectListItem>();
            foreach (var element in allElements)
            {
                elementsList.Add(new SelectListItem
                {
                    Value = element.id.ToString(),
                    Text = element.name,
                    Selected = elementIds.Contains(element.id)
                });
            }
            return elementsList;
        }
    }
}