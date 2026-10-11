namespace Libiada.Web.Extensions;

using Libiada.Core.Exceptions;
using Libiada.Core.Extensions;
using Libiada.Web.Models.CalculatorsData;

using static Libiada.Database.Extensions.EnumExtensions;

/// <summary>
/// The enum helper.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converts given enum into select list.
    /// </summary>
    /// <typeparam name="T">
    /// Enum type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IEnumerable{Microsoft.AspNetCore.Mvc.Rendering.SelectListItem}"/>.
    /// </returns>
    /// /// <remarks>
    /// Works only with byte enums.
    /// </remarks>
    public static IEnumerable<SelectListItem> GetSelectList<T>()
    where T : struct, Enum => GetSelectList(selectedValues: Array.Empty<T>());

    /// <summary>
    /// Converts given enum into select list.
    /// </summary>
    /// <param name="selectedValues">
    /// List of enum values that sould be celected in SelectList.
    /// </param>
    /// <typeparam name="T">
    /// Enum type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IEnumerable{Microsoft.AspNetCore.Mvc.Rendering.SelectListItem}"/>.
    /// </returns>
    /// /// <remarks>
    /// Works only with byte enums.
    /// </remarks>
    public static IEnumerable<SelectListItem> GetSelectList<T>(IEnumerable<T> selectedValues)
    where T : struct, Enum => Enum.GetValues<T>().ToSelectList(selectedValues);

    /// <summary>
    /// Converts array of enum values into SelectList.
    /// </summary>
    /// <param name="values">
    /// The values.
    /// </param>
    /// <typeparam name="T">
    /// Enum type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IEnumerable{Microsoft.AspNetCore.Mvc.Rendering.SelectListItem}"/>.
    /// </returns>
    /// <exception cref="TypeArgumentException">
    /// Thrown if type argument is not enum.
    /// </exception>
    /// <remarks>
    /// Works only with byte enums.
    /// </remarks>
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> values)
        where T : struct, Enum => values.ToSelectList<T>([]);

    /// <summary>
    /// Converts array of enum values into SelectList.
    /// </summary>
    /// <param name="values">
    /// The values.
    /// </param>
    /// <param name="selectedValues">
    /// The selected Values.
    /// </param>
    /// <typeparam name="T">
    /// Enum type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IEnumerable{Microsoft.AspNetCore.Mvc.Rendering.SelectListItem}"/>.
    /// </returns>
    /// <exception cref="TypeArgumentException">
    /// Thrown if type argument is not enum.
    /// </exception>
    /// <remarks>
    /// Works only with byte enums.
    /// </remarks>
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> values, IEnumerable<T> selectedValues)
        where T : struct, Enum
    {
        return values.Select(e => new SelectListItem
        {
            Value = Convert.ToByte(e).ToString(),
            Text = e.GetDisplayValue(),
            Selected = selectedValues.Contains(e)
        });
    }

    /// <summary>
    /// Converts array of enum values into SelectListWithNature.
    /// </summary>
    /// <param name="values">
    /// The values.
    /// </param>
    /// <typeparam name="T">
    /// Enum type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IEnumerable{Libiada.Web.Models.CalculatorsData.SelectListItemWithNature}"/>.
    /// </returns>
    /// <exception cref="TypeArgumentException">
    /// Thrown if type argument is not enum.
    /// </exception>
    /// <remarks>
    /// Works only with byte enums.
    /// </remarks>
    public static IEnumerable<SelectListItemWithNature> ToSelectListWithNature<T>(this IEnumerable<T> values)
        where T : struct, Enum => values.ToSelectListWithNature<T>([]);

    /// <summary>
    /// Converts array of enum values into SelectListWithNature.
    /// </summary>
    /// <param name="values">
    /// The values.
    /// </param>
    /// <param name="selectedValues">
    /// The selected values.
    /// </param>
    /// <typeparam name="T">
    /// Enum type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IEnumerable{Libiada.Web.Models.CalculatorsData.SelectListItemWithNature}"/>.
    /// </returns>
    /// <exception cref="TypeArgumentException">
    /// Thrown if type argument is not enum.
    /// </exception>
    /// <remarks>
    /// Works only with byte enums.
    /// </remarks>
    public static IEnumerable<SelectListItemWithNature> ToSelectListWithNature<T>(this IEnumerable<T> values, IEnumerable<T> selectedValues)
        where T : struct, Enum
    {
        return values.Select(e => new SelectListItemWithNature
        {
            Value = Convert.ToByte(e).ToString(),
            Text = e.GetDisplayValue(),
            Selected = selectedValues.Contains(e),
            Nature = (byte)e.GetNature()
        });
    }
}
