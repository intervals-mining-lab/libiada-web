namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Models.CalculatorsData;

    /// <summary>
    /// The notation repository.
    /// </summary>
    public class NotationRepository : INotationRepository
    {
        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature()
        {
            return EnumExtensions.ToArray<Notation>().Select(n => new SelectListItemWithNature
            {
                Value = ((byte)n).ToString(),
                Text = n.GetDisplayValue(),
                Selected = false,
                Nature = (byte)n.GetNature()
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
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature(Notation selectedNotation)
        {
            return EnumExtensions.ToArray<Notation>().Select(n => new SelectListItemWithNature
            {
                Value = ((byte)n).ToString(),
                Text = n.GetDisplayValue(),
                Selected = n == selectedNotation,
                Nature = (byte)n.GetNature()
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="notations">
        /// The notation ids.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature(List<Notation> notations)
        {
            return notations.Select(n => new SelectListItemWithNature
            {
                Value = ((byte)n).ToString(),
                Text = n.GetDisplayValue(),
                Selected = false,
                Nature = (byte)n.GetNature()
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="notations">
        /// The notation ids.
        /// </param>
        /// <param name="selectedNotation">
        /// The selected Notation.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature(List<Notation> notations, Notation selectedNotation)
        {
            return notations.Select(n => new SelectListItemWithNature
            {
                Value = ((byte)n).ToString(),
                Text = n.GetDisplayValue(),
                Selected = n == selectedNotation,
                Nature = (byte)n.GetNature()
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
