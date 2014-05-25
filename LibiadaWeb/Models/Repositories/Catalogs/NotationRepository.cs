namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// The notation repository.
    /// </summary>
    public class NotationRepository : INotationRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotationRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public NotationRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="notations">
        /// The notations.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<notation> notations)
        {
            HashSet<int> notationIds = notations != null
                                           ? new HashSet<int>(notations.Select(c => c.id))
                                           : new HashSet<int>();
            var allNotations = this.db.notation;
            var notationsList = new List<SelectListItem>();
            foreach (var notation in allNotations)
            {
                notationsList.Add(new SelectListItem
                    {
                        Value = notation.id.ToString(), 
                        Text = notation.name, 
                        Selected = notationIds.Contains(notation.id)
                    });
            }

            return notationsList;
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return this.db.notation.Select(n => new
            {
                Value = n.id, 
                Text = n.name, 
                Selected = false, 
                Nature = n.nature_id
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedNotation">
        /// The selected notation.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(int selectedNotation)
        {
            return this.db.notation.Select(n => new
            {
                Value = n.id, 
                Text = n.name, 
                Selected = n.id == selectedNotation, 
                Nature = n.nature_id
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedNotations">
        /// The selected notations.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(List<int> selectedNotations)
        {
            return this.db.notation.Select(n => new
            {
                Value = n.id, 
                Text = n.name, 
                Selected = selectedNotations.Contains(n.id), 
                Nature = n.nature_id
            });
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