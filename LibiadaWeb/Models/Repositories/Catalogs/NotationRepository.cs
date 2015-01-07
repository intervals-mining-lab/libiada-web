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
        /// The <see cref="List{SelectListItem}"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<Notation> notations)
        {
            HashSet<int> notationIds = notations != null
                                           ? new HashSet<int>(notations.Select(c => c.Id))
                                           : new HashSet<int>();
            var allNotations = db.Notation;
            var notationsList = new List<SelectListItem>();
            foreach (var notation in allNotations)
            {
                notationsList.Add(new SelectListItem
                    {
                        Value = notation.Id.ToString(), 
                        Text = notation.Name, 
                        Selected = notationIds.Contains(notation.Id)
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
            return db.Notation.Select(n => new
            {
                Value = n.Id, 
                Text = n.Name, 
                Selected = false, 
                Nature = n.NatureId
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
            return db.Notation.Select(n => new
            {
                Value = n.Id, 
                Text = n.Name, 
                Selected = n.Id == selectedNotation, 
                Nature = n.NatureId
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
            return db.Notation.Select(n => new
            {
                Value = n.Id, 
                Text = n.Name, 
                Selected = selectedNotations.Contains(n.Id), 
                Nature = n.NatureId
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
