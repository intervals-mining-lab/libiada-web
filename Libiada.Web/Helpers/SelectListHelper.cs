namespace Libiada.Web.Helpers;

using Libiada.Web.Models.CalculatorsData;

public static class SelectListHelper
{
    /// <summary>
    /// Creates list of matter table rows.
    /// </summary>
    /// <param name="cache">
    /// Matters in-memory cache..
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{MattersTableRow}"/>.
    /// </returns>
    public static IEnumerable<MattersTableRow> GetMatterSelectList(Cache cache)
    {
        return GetMatterSelectList(m => true, m => false, cache);
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
    /// <param name="cache">
    /// Matters in-memory cache.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{MattersTableRow}"/>.
    /// </returns>
    public static IEnumerable<MattersTableRow> GetMatterSelectList(Func<Matter, bool> filter, Func<Matter, bool> selectionFilter, Cache cache)
    {
        return GetMatterSelectList(cache.Matters.Where(filter), selectionFilter);
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
    /// Creates list of sequence groups table rows.
    /// </summary>
    /// <param name="db">
    /// The database connection.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{MattersTableRow}"/>.
    /// </returns>
    public static IEnumerable<MattersTableRow> GetSequenceGroupSelectList(Func<SequenceGroup, bool> filter, LibiadaDatabaseEntities db)
    {
        return db.SequenceGroups.Where(filter).OrderBy(m => m.Created).Select(sg => new MattersTableRow(sg, false)).ToArray();
    }

    /// <summary>
    /// Creates list of sequence groups table rows.
    /// </summary>
    /// <param name="db">
    /// The database connection.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{MattersTableRow}"/>.
    /// </returns>
    public static IEnumerable<MattersTableRow> GetSequenceGroupSelectList(LibiadaDatabaseEntities db)
    {
        return GetSequenceGroupSelectList(sg => true, db);
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
        return db.Multisequences.Select(ms => new SelectListItemWithNature
        {
            Value = ms.Id.ToString(), 
            Text = ms.Name, 
            Nature = (byte)ms.Nature 
        }).ToArray();
    }
}
