namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
        /// The create matter.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        public void CreateMatterFromSequence(CommonSequence commonSequence)
        {
            var matter = commonSequence.Matter;
            if (matter != null)
            {
                matter.Sequence = new Collection<CommonSequence>();
                commonSequence.MatterId = CreateMatter(matter);
            }
        }

        /// <summary>
        /// The create matter.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        public long CreateMatter(Matter matter)
        {
            db.Matter.Add(matter);
            db.SaveChanges();
            return matter.Id;
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="selectedMatters">
        /// The selected matters.
        /// </param>
        /// <returns>
        /// The <see cref="List{SelectListItem}"/>.
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
        /// The <see cref="List{SelectListItem}"/>.
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
        public IEnumerable<object> GetMatterSelectList()
        {
            return GetMatterSelectList(new List<long>());
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
        public IEnumerable<object> GetMatterSelectList(long selectedMatter)
        {
            return GetMatterSelectList(new List<long> { selectedMatter });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedMatters">
        /// The selected matters.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetMatterSelectList(IEnumerable<long> selectedMatters)
        {
            return db.Matter.OrderBy(m => m.Name).Select(m => new
            {
                Value = m.Id, 
                Text = m.Name, 
                Selected = selectedMatters.Contains(m.Id), 
                Nature = m.NatureId, 
                Description = m.Description,
                Created = m.Created.ToString(),
                Modified = m.Modified.ToString()
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="matters">
        /// The matters.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetMatterSelectList(IEnumerable<Matter> matters)
        {
            return matters.OrderBy(m => m.Name).Select(m => new
            {
                Value = m.Id,
                Text = m.Name,
                Selected = false,
                Nature = m.NatureId,
                Description = m.Description,
                Created = m.Created.ToString(),
                Modified = m.Modified.ToString()
            });
        }

        /// <summary>
        /// The get matter select list.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetMatterSelectList(Func<Matter, bool> filter)
        {
            return GetMatterSelectList(db.Matter.Where(filter));
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
