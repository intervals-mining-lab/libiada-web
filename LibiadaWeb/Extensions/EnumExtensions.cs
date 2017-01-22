namespace LibiadaWeb.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Exceptions;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Attributes;
    using LibiadaWeb.Models.CalculatorsData;

    /// <summary>
    /// The enum helper.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets nature attribute value for given enum value.
        /// </summary>
        /// <typeparam name="T">
        /// Enum with nature attribute.
        /// </typeparam>
        /// <param name="value">
        /// Enum value.
        /// </param>
        /// <returns>
        /// Nature attribute value as <see cref="Nature"/>
        /// </returns>
        public static Nature GetNature<T>(this T value) where T : struct, IComparable, IFormattable, IConvertible
        {
            return value.GetAttribute<T, NatureAttribute>().Value;
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
        /// The <see cref="IEnumerable{SelectListItem}"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        /// <remarks>
        /// Works only with byte enums.
        /// </remarks>
        public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> values, bool useDisplayValueAsValue = false)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            return values.Select(e => new SelectListItem
                {
                    Value = useDisplayValueAsValue ? e.GetDisplayValue() : Convert.ToByte(e).ToString(),
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
        /// The <see cref="IEnumerable{SelectListItem}"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        /// <remarks>
        /// Works only with byte enums.
        /// </remarks>
        public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> values, IEnumerable<T> selectedValues, bool useDisplayValueAsValue = false)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            return values.Select(e => new SelectListItem
                {
                    Value = useDisplayValueAsValue ? e.GetDisplayValue() : Convert.ToByte(e).ToString(),
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
        /// The <see cref="IEnumerable{SelectListItemWithNature}"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        /// <remarks>
        /// Works only with byte enums.
        /// </remarks>
        public static IEnumerable<SelectListItemWithNature> ToSelectListWithNature<T>(this IEnumerable<T> values, bool useDisplayValueAsValue = false)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            return values.Select(e => new SelectListItemWithNature
                {
                    Value = useDisplayValueAsValue ? e.GetDisplayValue() : Convert.ToByte(e).ToString(),
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
        /// The <see cref="IEnumerable{SelectListItemWithNature}"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        /// <remarks>
        /// Works only with byte enums.
        /// </remarks>
        public static IEnumerable<SelectListItemWithNature> ToSelectListWithNature<T>(this IEnumerable<T> values, IEnumerable<T> selectedValues, bool useDisplayValueAsValue = false)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            return values.Select(e => new SelectListItemWithNature
                {
                    Value = useDisplayValueAsValue ? e.GetDisplayValue() : Convert.ToByte(e).ToString(),
                    Text = e.GetDisplayValue(),
                    Selected = selectedValues.Contains(e),
                    Nature = (byte)e.GetNature()
                });
        }
    }
}
