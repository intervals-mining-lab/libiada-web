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
                Nature = m.nature_id,
                description = m.description
            });
        }

        public IEnumerable<object> GetSelectListWithNature(long selectedMatter)
        {
            return db.matter.Select(m => new
            {
                Value = m.id,
                Text = m.name,
                Selected = m.id == selectedMatter,
                Nature = m.nature_id,
                description = m.description
            });
        }

        public IEnumerable<object> GetSelectListWithNature(IEnumerable<long> selectedMatters)
        {
            return db.matter.Select(m => new
            {
                Value = m.id,
                Text = m.name,
                Selected = selectedMatters.Contains(m.id),
                Nature = m.nature_id,
                description = m.description
            });
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}