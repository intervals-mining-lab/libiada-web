namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// The link repository.
    /// </summary>
    public class LinkRepository : ILinkRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public LinkRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="links">
        /// The links.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<link> links)
        {
            HashSet<int> linkIds = links != null
                                         ? new HashSet<int>(links.Select(c => c.id))
                                         : new HashSet<int>();
            var allLinks = this.db.link;
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

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.db.Dispose();
        }
    }
}