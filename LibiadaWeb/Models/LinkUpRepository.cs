using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models
{ 
    public class LinkUpRepository : ILinkUpRepository
    {
        private readonly LibiadaWebEntities db;


        public LinkUpRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }
        public IQueryable<link_up> All
        {
            get { return db.link_up; }
        }

        public IQueryable<link_up> AllIncluding(params Expression<Func<link_up, object>>[] includeProperties)
        {
            IQueryable<link_up> query = db.link_up;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public link_up Find(int id)
        {
            return db.link_up.Single(x => x.id == id);
        }

        public void InsertOrUpdate(link_up link_up)
        {
            if (link_up.id == default(int)) {
                // New entity
                db.link_up.AddObject(link_up);
            } else {
                // Existing entity
                db.link_up.Attach(link_up);
                db.ObjectStateManager.ChangeObjectState(link_up, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var link_up = Find(id);
            db.link_up.DeleteObject(link_up);
        }

        public void Save()
        {
            db.SaveChanges();
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
            var allLinkUps = db.link_up;
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
            db.Dispose();
        }
    }
}