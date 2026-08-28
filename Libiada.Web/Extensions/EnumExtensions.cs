namespace Libiada.Web.Extensions;

using Libiada.Core.Exceptions;
using Libiada.Core.Extensions;
using static Libiada.Database.Extensions.EnumExtensions;
using Libiada.Web.Models.CalculatorsData;

/// <summary>
/// The enum helper.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converts given enum into select list.
    /// </summary>
    /// <param name="useDisplayValueAsValue">
    /// If true all values of select list are display values of enum values.
    /// Otherwise uses byte enum value.
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
    public static IEnumerable<SelectListItem> GetSelectList<T>(bool useDisplayValueAsValue = false)
    where T : struct, Enum
    {
        return GetSelectList(selectedValues: Array.Empty<T>(), useDisplayValueAsValue);
    }

    /// <summary>
    /// Converts given enum into select list.
    /// </summary>
    /// <param name="selectedValues">
    /// List of enum values that sould be celected in SelectList.
    /// </param>
    /// <param name="useDisplayValueAsValue">
    /// If true all values of select list are display values of enum values.
    /// Otherwise uses byte enum value.
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
    public static IEnumerable<SelectListItem> GetSelectList<T>(IEnumerable<T> selectedValues, bool useDisplayValueAsValue = false)
    where T : struct, Enum
    {
        T[] values = Enum.GetValues<T>();
        return values.ToSelectList(selectedValues, useDisplayValueAsValue);
    }

    /// <summary>
    /// Converts array of enum values into SelectList.
    /// </summary>
    /// <param name="values">
    /// The values.
    /// </param>
    /// <param name="useDisplayValueAsValue">
    /// If true all values of select list are display values of enum values.
    /// Otherwise uses byte enum value.
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
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> values, bool useDisplayValueAsValue = false)
        where T : struct, Enum
    {
        return values.Select(e => new SelectListItem
        {
            Value = useDisplayValueAsValue ? e.GetDisplayValue() : Enum.GetName(e),
            Text = e.GetDisplayValue(),
            Selected = false
        });
    }

    /// <summary>
    /// Converts array of enum values into SelectList.
    /// </summary>
    /// <param name="values">
    /// The values.
    /// </param>
    /// <param name="selectedValues">
    /// The selected Values.
    /// </param>
    /// <param name="useDisplayValueAsValue">
    /// If true all values of select list are display values of enum values.
    /// Otherwise uses byte enum value.
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
    public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> values, IEnumerable<T> selectedValues, bool useDisplayValueAsValue = false)
        where T : struct, Enum
    {
        return values.Select(e => new SelectListItem
        {
            Value = useDisplayValueAsValue ? e.GetDisplayValue() : Enum.GetName(e),
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
    /// <param name="useDisplayValueAsValue">
    /// If true all values of select list are display values of enum values.
    /// Otherwise uses byte enum value.
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
    public static IEnumerable<SelectListItemWithNature> ToSelectListWithNature<T>(this IEnumerable<T> values, bool useDisplayValueAsValue = false)
        where T : struct, Enum
    {
        return values.Select(e => new SelectListItemWithNature
        {
            Value = useDisplayValueAsValue ? e.GetDisplayValue() : Enum.GetName(e),
            Text = e.GetDisplayValue(),
            Selected = false,
            Nature = (byte)e.GetNature()
        });
    }

    /// <summary>
    /// Converts array of enum values into SelectListWithNature.
    /// </summary>
    /// <param name="values">
    /// The values.
    /// </param>
    /// <param name="selectedValues">
    /// The selected values.
    /// </param>
    /// <param name="useDisplayValueAsValue">
    /// If true all values of select list are display values of enum values.
    /// Otherwise uses byte enum value.
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
    public static IEnumerable<SelectListItemWithNature> ToSelectListWithNature<T>(this IEnumerable<T> values, IEnumerable<T> selectedValues, bool useDisplayValueAsValue = false)
        where T : struct, Enum
    {
        return values.Select(e => new SelectListItemWithNature
        {
            Value = useDisplayValueAsValue ? e.GetDisplayValue() : Enum.GetName(e),
            Text = e.GetDisplayValue(),
            Selected = selectedValues.Contains(e),
            Nature = (byte)e.GetNature()
        });
    }
}
