namespace Libiada.Web.Models;

using Libiada.Web.Models.CalculatorsData;

/// <summary>
/// Comparer for <see cref="SelectListItemWithNature"/>.
/// </summary>
public class SelectListItemWithNatureComparer : IEqualityComparer<SelectListItemWithNature>
{
    /// <summary>
    /// Compares two SelectListItemWithNature objects.
    /// </summary>
    /// <param name="x">
    /// The first select list item with nature.
    /// </param>
    /// <param name="y">
    /// The second select list item with nature.
    /// </param>
    /// <returns>
    /// Comparing result <see cref="bool"/>.
    /// </returns>
    public bool Equals(SelectListItemWithNature? x, SelectListItemWithNature? y)
    {
        return (x == y) || (!(x == null || y == null) &&
            (x.Text == y.Text &&
             x.Value == y.Value &&
             x.Selected == y.Selected &&
             x.Nature == y.Nature));
    }

    /// <summary>
    /// Calculates hash code based on Text, Value, Selected flag and Nature.
    /// </summary>
    /// <param name="item">
    /// SelectListItemWithNature object to calculatee hash code for.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public int GetHashCode(SelectListItemWithNature item)
    {
        int hashText = item.Text == null ? 0 : item.Text.GetHashCode();
        int hashValue = item.Value == null ? 0 : item.Value.GetHashCode();
        int hashSelected = item.Selected ? 1 : 0;
        return hashText ^ hashValue ^ hashSelected ^ item.Nature;
    }
}
