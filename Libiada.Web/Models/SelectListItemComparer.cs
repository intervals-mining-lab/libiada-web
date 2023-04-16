namespace Libiada.Web.Models
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The select list item comparer.
    /// </summary>
    public class SelectListItemComparer : IEqualityComparer<SelectListItem>
    {
        /// <summary>
        /// To compare the select list items.
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
        public bool Equals(SelectListItem x, SelectListItem y)
        {
            return (x == y) || (!(x == null || y == null) && (x.Text == y.Text && x.Value == y.Value));
        }

        /// <summary>
        /// To get hash code.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetHashCode(SelectListItem item)
        {
            int hashText = item.Text == null ? 0 : item.Text.GetHashCode();
            int hashValue = item.Value == null ? 0 : item.Value.GetHashCode();
            return hashText ^ hashValue;
        }
    }
}