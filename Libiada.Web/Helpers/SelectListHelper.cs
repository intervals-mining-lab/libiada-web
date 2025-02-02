namespace Libiada.Web.Helpers;

using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Web.Models.CalculatorsData;

public static class SelectListHelper
{
    /// <summary>
    /// Creates list of research object table rows.
    /// </summary>
    /// <param name="cache">
    /// Research objects in-memory cache..
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{Libiada.Web.Models.CalculatorsData.ResearchObjectTableRow}"/>.
    /// </returns>
    public static IEnumerable<ResearchObjectTableRow> GetResearchObjectSelectList(IResearchObjectsCache cache)
    {
        return GetResearchObjectSelectList(m => true, m => false, cache);
    }

    /// <summary>
    /// Creates filtered list of research object table rows.
    /// </summary>
    /// <param name="filter">
    /// The research objects filter.
    /// </param>
    /// /// <param name="selectionFilter">
    /// The research objects selection filter.
    /// </param>
    /// <param name="cache">
    /// Research objects in-memory cache.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{Libiada.Web.Models.CalculatorsData.ResearchObjectTableRow}"/>.
    /// </returns>
    public static IEnumerable<ResearchObjectTableRow> GetResearchObjectSelectList(Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selectionFilter, IResearchObjectsCache cache)
    {
        return GetResearchObjectSelectList(cache.ResearchObjects.Where(filter), selectionFilter);
    }

    /// <summary>
    /// Creates list of research object table rows from given research objects
    /// with given selection condition.
    /// </summary>
    /// <param name="researchObjects">
    /// The research objects.
    /// </param>
    /// <param name="selected">
    /// The sselection condition.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{Libiada.Web.Models.CalculatorsData.ResearchObjectTableRow}"/>.
    /// </returns>
    public static IEnumerable<ResearchObjectTableRow> GetResearchObjectSelectList(IEnumerable<ResearchObject> researchObjects, Func<ResearchObject, bool> selected)
    {
        return researchObjects.OrderBy(m => m.Created).Select(m => new ResearchObjectTableRow(m, selected(m)));
    }

    /// <summary>
    /// Creates list of sequence groups table rows.
    /// </summary>
    /// <param name="db">
    /// The database connection.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{Libiada.Web.Models.CalculatorsData.ResearchObjectTableRow}"/>.
    /// </returns>
    public static IEnumerable<ResearchObjectTableRow> GetSequenceGroupSelectList(Func<SequenceGroup, bool> filter, LibiadaDatabaseEntities db)
    {
        return db.SequenceGroups.Where(filter).OrderBy(m => m.Created).Select(sg => new ResearchObjectTableRow(sg, false)).ToArray();
    }

    /// <summary>
    /// Creates list of sequence groups table rows.
    /// </summary>
    /// <param name="db">
    /// The database connection.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{Libiada.Web.Models.CalculatorsData.ResearchObjectTableRow}"/>.
    /// </returns>
    public static IEnumerable<ResearchObjectTableRow> GetSequenceGroupSelectList(LibiadaDatabaseEntities db)
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
    ///  The <see cref="IEnumerable{Libiada.Web.Models.CalculatorsData.SelectListItemWithNature}"/>.
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
