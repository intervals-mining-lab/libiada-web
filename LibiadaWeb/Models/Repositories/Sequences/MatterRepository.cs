namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using LibiadaWeb.Extensions;

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
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetMatterSelectList()
        {
            return GetMatterSelectList(m => true);
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
                Created = m.Created.ToString(),
                Modified = m.Modified.ToString(),
                SequenceType = m.SequenceType.GetDisplayValue(),
                Group = m.Group.GetDisplayValue(),
                m.Nature,
                m.Description
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
            return matters.OrderBy(m => m.Created).Select(m => new
            {
                Value = m.Id,
                Text = m.Name,
                Selected = false,
                Created = m.Created.ToString(),
                Modified = m.Modified.ToString(),
                SequenceType = m.SequenceType.GetDisplayValue(),
                Group = m.Group.GetDisplayValue(),
                m.Nature,
                m.Description
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

        /// <summary>
        /// The create matter.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        private long CreateMatter(Matter matter)
        {
            db.Matter.Add(matter);
            db.SaveChanges();
            return matter.Id;
        }
    }
}
