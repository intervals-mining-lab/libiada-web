namespace Libiada.Web.Models;

/// <summary>
/// Comparer for <see cref="SelectListItem"/>.
/// </summary>
public class SelectListItemComparer : IEqualityComparer<SelectListItem>
{
    /// <summary>
    /// Compares two SelectListItem objects.
    /// </summary>
    /// <param name="x">
    /// The first select list item.
    /// </param>
    /// <param name="y">
    /// The second select list item.
    /// </param>
    /// <returns>
    /// Comparing result <see cref="bool"/>.
    /// </returns>
    public bool Equals(SelectListItem? x, SelectListItem? y)
    {
        return (x == y) || (!(x == null || y == null) && (x.Text == y.Text && x.Value == y.Value && x.Selected == y.Selected));
    }

    /// <summary>
    /// Calculates hash code based on Text, Value, and Selected flag.
    /// </summary>
    /// <param name="item">
    /// SelectListItem object to calculatee hash code for.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public int GetHashCode(SelectListItem item)
    {
        int hashText = item.Text == null ? 0 : item.Text.GetHashCode();
        int hashValue = item.Value == null ? 0 : item.Value.GetHashCode();
        int hashSelected = item.Selected ? 1 : 0;
        return hashText ^ hashValue ^ hashSelected;
    }
}
