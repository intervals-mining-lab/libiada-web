using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using LibiadaWeb.Models.Repositories.Interfaces;

namespace LibiadaWeb.Models.Repositories
{ 
    public class MatterRepository : IMatterRepository
    {
        private readonly LibiadaWebEntities db;

        public MatterRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<matter> All
        {
            get { return db.matter; }
        }

        public IQueryable<matter> AllIncluding(params Expression<Func<matter, object>>[] includeProperties)
        {
            IQueryable<matter> query = db.matter;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public matter Find(long id)
        {
            return db.matter.Single(x => x.id == id);
        }

        public void InsertOrUpdate(matter matter)
        {
            if (matter.id == default(long)) {
                // New entity
                db.matter.AddObject(matter);
            } else {
                // Existing entity
                db.matter.Attach(matter);
                db.ObjectStateManager.ChangeObjectState(matter, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var matter = Find(id);
            db.matter.DeleteObject(matter);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> matters)
        {
            HashSet<long> matterIds;
            if (matters != null)
            {
                matterIds = new HashSet<long>(matters.Select(c => c.id));
            }
            else
            {
                matterIds = new HashSet<long>();
            }
            var allMatters = db.matter;
            var mattersList = new List<SelectListItem>();
            foreach (var matter in allMatters)
            {
                mattersList.Add(new SelectListItem
                {
                    Value = matter.id.ToString(),
                    Text = matter.name,
                    Selected = matterIds.Contains(matter.id)
                });
            }
            return mattersList;
        }

        public void Dispose() 
        {
            db.Dispose();
        }
    }
}