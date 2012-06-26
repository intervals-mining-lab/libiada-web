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
    public class LinkUpRepository : ILinkUpRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<link_up> All
        {
            get { return context.link_up; }
        }

        public IQueryable<link_up> AllIncluding(params Expression<Func<link_up, object>>[] includeProperties)
        {
            IQueryable<link_up> query = context.link_up;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public link_up Find(int id)
        {
            return context.link_up.Single(x => x.id == id);
        }

        public void InsertOrUpdate(link_up link_up)
        {
            if (link_up.id == default(int)) {
                // New entity
                context.link_up.AddObject(link_up);
            } else {
                // Existing entity
                context.link_up.Attach(link_up);
                context.ObjectStateManager.ChangeObjectState(link_up, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var link_up = context.link_up.Single(x => x.id == id);
            context.link_up.DeleteObject(link_up);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<link_up> linkUps)
        {
            HashSet<int> linkUpIds;
            if (linkUps != null)
            {
                linkUpIds = new HashSet<int>(linkUps.Select(c => c.id));
            }
            else
            {
                linkUpIds = new HashSet<int>();
            }
            var allLinkUps = context.link_up;
            var linkUpsList = new List<SelectListItem>();
            foreach (var linkUp in allLinkUps)
            {
                linkUpsList.Add(new SelectListItem
                {
                    Value = linkUp.id.ToString(),
                    Text = linkUp.name,
                    Selected = linkUpIds.Contains(linkUp.id)
                });
            }
            return linkUpsList;
        }

        public void Dispose() 
        {
            context.Dispose();
        }
    }
}