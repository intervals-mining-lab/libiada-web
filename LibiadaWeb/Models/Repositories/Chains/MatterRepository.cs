using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories.Chains
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
            foreach (var includeProperty in includeProperties)
            {
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
            if (matter.id == default(long))
            {
                // New entity
                db.matter.AddObject(matter);
            }
            else
            {
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

        public List<SelectListItem> GetSelectListItems(IEnumerable<matter> selectedMatters)
        {
            return GetSelectListItems(db.matter, selectedMatters);
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<matter> allMatters, IEnumerable<matter> selectedMatters)
        {

            HashSet<long> matterIds = selectedMatters != null
                                          ? new HashSet<long>(selectedMatters.Select(c => c.id))
                                          : new HashSet<long>();
            var mattersList = new List<SelectListItem>();
            if (allMatters == null)
            {
                allMatters = db.matter;
            }
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

        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.matter.Select(m => new
            {
                Value = m.id,
                Text = m.name,
                Selected = false,
                Nature = m.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(long selectedMatter)
        {
            return db.matter.Select(m => new
            {
                Value = m.id,
                Text = m.name,
                Selected = m.id == selectedMatter,
                Nature = m.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(IEnumerable<long> selectedMatters)
        {
            return db.matter.Select(m => new
            {
                Value = m.id,
                Text = m.name,
                Selected = selectedMatters.Contains(m.id),
                Nature = m.nature_id
            });
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}