namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Exceptions;

    /// <summary>
    /// The enum helper.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// The get display value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// Enum type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetDisplayValue<T>(this T value) where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            var fieldInfo = type.GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null)
            {
                return string.Empty;
            }

            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
        }

        /// <summary>
        /// The get name.
        /// </summary>
        /// <param name="value">
        /// The nature.
        /// </param>
        /// <typeparam name="T">
        /// Enum type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        public static string GetName<T>(this T value) where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            return Enum.GetName(type, value);
        }

        /// <summary>
        /// The to array.
        /// </summary>
        /// <typeparam name="T">
        /// Enum type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T:T[]"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        public static T[] ToArray<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            return (T[])Enum.GetValues(type);
        }

        /// <summary>
        /// The to select list.
        /// </summary>
        /// <typeparam name="T">
        /// Enum type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="SelectList"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        public static SelectList ToSelectList<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            var values = ToArray<T>();
            return ToSelectList(values);
        }

        /// <summary>
        /// The to select list.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <typeparam name="T">
        /// Enum type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="SelectList"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        public static SelectList ToSelectList<T>(this IEnumerable<T> values) where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            return new SelectList(values.Select(e => new { id = Convert.ToByte(e), name = GetName(e) }), "id", "name");
        }

        /// <summary>
        /// The to select list.
        /// </summary>
        /// <param name="values">
        /// Enum values.
        /// </param>
        /// <param name="selected">
        /// Selected value.
        /// </param>
        /// <typeparam name="T">
        /// Enum type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="SelectList"/>.
        /// </returns>
        /// <exception cref="TypeArgumentException">
        /// Thrown if type argument is not enum.
        /// </exception>
        public static SelectList ToSelectList<T>(this IEnumerable<T> values, T selected) where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            var list = values.Select(e => new { id = Convert.ToByte(e), name = GetName(e) });
            return new SelectList(list, "id", "name", Convert.ToByte(selected));
        }
    }
}
