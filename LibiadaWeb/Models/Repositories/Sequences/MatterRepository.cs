namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// The matter repository.
    /// </summary>
    public class MatterRepository : IMatterRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatterRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public MatterRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="selectedMatters">
        /// The selected matters.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<Matter> selectedMatters)
        {
            return GetSelectListItems(db.Matter, selectedMatters);
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="allMatters">
        /// The all matters.
        /// </param>
        /// <param name="selectedMatters">
        /// The selected matters.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<Matter> allMatters, IEnumerable<Matter> selectedMatters)
        {
            HashSet<long> matterIds = selectedMatters != null
                                          ? new HashSet<long>(selectedMatters.Select(c => c.Id))
                                          : new HashSet<long>();
            var mattersList = new List<SelectListItem>();
            if (allMatters == null)
            {
                allMatters = db.Matter;
            }

            foreach (var matter in allMatters)
            {
                mattersList.Add(new SelectListItem
                    {
                        Value = matter.Id.ToString(), 
                        Text = matter.Name, 
                        Selected = matterIds.Contains(matter.Id)
                    });
            }

            return mattersList;
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.Matter.OrderBy(m => m.Name).Select(m => new
            {
                Value = m.Id, 
                Text = m.Name, 
                Selected = false, 
                Nature = m.NatureId, 
                description = m.Description,
                created = m.Created.ToString()
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedMatter">
        /// The selected matter.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(long selectedMatter)
        {
            return db.Matter.Select(m => new
            {
                Value = m.Id, 
                Text = m.Name, 
                Selected = m.Id == selectedMatter, 
                Nature = m.NatureId, 
                description = m.Description
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedMatters">
        /// The selected matters.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<long> selectedMatters)
        {
            return db.Matter.Select(m => new
            {
                Value = m.Id, 
                Text = m.Name, 
                Selected = selectedMatters.Contains(m.Id), 
                Nature = m.NatureId, 
                description = m.Description
            });
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
