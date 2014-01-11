using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class LinkRepository : ILinkRepository
    {
        private readonly LibiadaWebEntities db;


        public LinkRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<link> All
        {
            get { return db.link; }
        }

        public IQueryable<link> AllIncluding(params Expression<Func<link, object>>[] includeProperties)
        {
            IQueryable<link> query = db.link;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public link Find(int id)
        {
            return db.link.Single(x => x.id == id);
        }

        public void InsertOrUpdate(link link)
        {
            if (link.id == default(int))
            {
                // New entity
                db.link.AddObject(link);
            }
            else
            {
                // Existing entity
                db.link.Attach(link);
                db.ObjectStateManager.ChangeObjectState(link, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var link = Find(id);
            db.link.DeleteObject(link);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<link> links)
        {
            HashSet<int> linkIds = links != null
                                         ? new HashSet<int>(links.Select(c => c.id))
                                         : new HashSet<int>();
            var allLinks = db.link;
            var linksList = new List<SelectListItem>();
            foreach (var link in allLinks)
            {
                linksList.Add(new SelectListItem
                    {
                        Value = link.id.ToString(),
                        Text = link.name,
                        Selected = linkIds.Contains(link.id)
                    });
            }
            return linksList;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}