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
        /// The notations.
        /// </summary>
        public readonly Notation[] Notations;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotationRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public NotationRepository(LibiadaWebEntities db)
        {
            Notations = db.Notation.ToArray();
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return Notations.Select(n => new
            {
                Value = n.Id,
                Text = n.Name,
                Selected = false,
                Nature = n.Nature
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
            return Notations.Select(n => new
            {
                Value = n.Id,
                Text = n.Name,
                Selected = n.Id == selectedNotation,
                Nature = n.Nature
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
            return Notations.Where(n => notationIds.Contains(n.Id)).Select(n => new
            {
                Value = n.Id,
                Text = n.Name,
                Selected = false,
                Nature = n.Nature
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
            return Notations.Where(n => notationIds.Contains(n.Id)).Select(n => new
            {
                Value = n.Id,
                Text = n.Name,
                Selected = n.Id == selectedNotation,
                Nature = n.Nature
            });
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
