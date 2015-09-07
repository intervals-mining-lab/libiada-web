namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

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
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
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
        /// The <see cref="IEnumerable{Object}"/>.
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
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(List<int> notationIds)
        {
            return db.Notation.Where(n => notationIds.Contains(n.Id)).Select(n => new
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
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="selectedNotation">
        /// The selected Notation.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(List<int> notationIds, int selectedNotation)
        {
            return db.Notation.Where(n => notationIds.Contains(n.Id)).Select(n => new
            {
                Value = n.Id,
                Text = n.Name,
                Selected = n.Id == selectedNotation,
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
