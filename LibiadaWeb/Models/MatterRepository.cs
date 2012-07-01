using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using LibiadaWeb;

namespace LibiadaWeb.Models
{ 
    public class MatterRepository : IMatterRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<matter> All
        {
            get { return context.matter; }
        }

        public IQueryable<matter> AllIncluding(params Expression<Func<matter, object>>[] includeProperties)
        {
            IQueryable<matter> query = context.matter;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public matter Find(long id)
        {
            return context.matter.Single(x => x.id == id);
        }

        public void InsertOrUpdate(matter matter)
        {
            if (matter.id == default(long)) {
                // New entity
                context.matter.AddObject(matter);
            } else {
                // Existing entity
                context.matter.Attach(matter);
                context.ObjectStateManager.ChangeObjectState(matter, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var matter = context.matter.Single(x => x.id == id);
            context.matter.DeleteObject(matter);
        }

        public void Save()
        {
            context.SaveChanges();
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
            var allMatters = context.matter;
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
            context.Dispose();
        }
    }
}