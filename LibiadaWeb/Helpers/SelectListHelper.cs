namespace LibiadaWeb.Helpers
{
    using Libiada.Database;
    using LibiadaWeb.Models.CalculatorsData;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class SelectListHelper
    {
        /// <summary>
        /// Creates list of matter table rows.
        /// </summary>
        /// <param name="db">
        /// The database connection.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{MattersTableRow}"/>.
        /// </returns>
        public static IEnumerable<MattersTableRow> GetMatterSelectList(LibiadaDatabaseEntities db)
        {
            return GetMatterSelectList(m => true, m => false, db);
        }

        /// <summary>
        /// Creates filtered list of matter table rows.
        /// </summary>
        /// <param name="filter">
        /// The matters filter.
        /// </param>
        /// /// <param name="selectionFilter">
        /// The matters selection filter.
        /// </param>
        /// <param name="db">
        /// The database connection.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{MattersTableRow}"/>.
        /// </returns>
        public static IEnumerable<MattersTableRow> GetMatterSelectList(Func<Matter, bool> filter, Func<Matter, bool> selectionFilter, LibiadaDatabaseEntities db)
        {
            return GetMatterSelectList(db.Matter.Where(filter), selectionFilter);
        }

        /// <summary>
        /// Creates list of matter table rows from given matters
        /// with given selection condition.
        /// </summary>
        /// <param name="matters">
        /// The matters.
        /// </param>
        /// <param name="selected">
        /// The sselection condition.
        /// </param>
        /// <returns>
        /// The <see cref="T:IEnumerable{MattersTableRow}"/>.
        /// </returns>
        public static IEnumerable<MattersTableRow> GetMatterSelectList(IEnumerable<Matter> matters, Func<Matter, bool> selected)
        {
            return matters.OrderBy(m => m.Created).Select(m => new MattersTableRow(m, selected(m)));
        }

        /// <summary>
        /// Creates select list of all multisequences.
        /// </summary>
        /// <param name="db">
        /// The database connection.
        /// </param>
        /// <returns>
        ///  The <see cref="T:IEnumerable{SelectListItemWithNature}"/>.
        /// </returns>
        public static IEnumerable<SelectListItemWithNature> GetMultisequenceSelectList(LibiadaDatabaseEntities db)
        {
            return db.Multisequence.Select(ms => new SelectListItemWithNature
            {
                Value = ms.Id.ToString(), 
                Text = ms.Name, 
                Nature = (byte)ms.Nature 
            });
        }
    }
}
