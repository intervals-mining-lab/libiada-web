namespace LibiadaWeb.Models.Repositories.Chains
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
        public List<SelectListItem> GetSelectListItems(IEnumerable<matter> selectedMatters)
        {
            return GetSelectListItems(db.matter, selectedMatters);
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

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.matter.OrderBy(m => m.name).Select(m => new
            {
                Value = m.id, 
                Text = m.name, 
                Selected = false, 
                Nature = m.nature_id, 
                description = m.description,
                created = m.created.ToString()
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedMatter">
        /// The selected matter.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
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
            return db.matter.Select(m => new
            {
                Value = m.id, 
                Text = m.name, 
                Selected = selectedMatters.Contains(m.id), 
                Nature = m.nature_id, 
                description = m.description
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