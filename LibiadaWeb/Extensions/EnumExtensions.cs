namespace LibiadaWeb.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Exceptions;

    using LibiadaWeb.Attributes;

    /// <summary>
    /// The enum helper.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets enum display name.
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
        /// Gets enum name as in code.
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
        /// Gets description attribute of the given enum value.
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
        public static string GetDescription<T>(this T value) where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = value.GetType();

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((DescriptionAttribute)attributes[0]).Description;
        }

        /// <summary>
        /// The get attribute.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// Enum type
        /// </typeparam>
        /// <typeparam name="TAttribute">
        /// Attribute type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="TAttribute"/>.
        /// </returns>
        public static TAttribute GetAttribute<T, TAttribute>(this T value)
            where T : struct, IComparable, IFormattable, IConvertible
            where TAttribute : Attribute
        {
            Type type = value.GetType();

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(TAttribute), false);
            return (attributes.Length > 0) ? (TAttribute)attributes[0] : null;
        }

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
        /// <remarks>
        /// Works only with byte enums.
        /// </remarks>
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
        /// <remarks>
        /// Works only with byte enums.
        /// </remarks>
        public static SelectList ToSelectList<T>(this IEnumerable<T> values) where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            return new SelectList(values.Select(e => new { id = Convert.ToByte(e), name = GetDisplayValue(e) }), "id", "name");
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
        /// <remarks>
        /// Works only with byte enums.
        /// </remarks>
        public static SelectList ToSelectList<T>(this IEnumerable<T> values, T? selected) where T : struct, IComparable, IFormattable, IConvertible
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeArgumentException("Type argument must be enum.");
            }

            var list = values.Select(e => new { id = Convert.ToByte(e), name = GetDisplayValue(e) });
            return new SelectList(list, "id", "name", Convert.ToByte(selected));
        }
    }
}
