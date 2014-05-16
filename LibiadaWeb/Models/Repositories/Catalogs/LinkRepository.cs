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