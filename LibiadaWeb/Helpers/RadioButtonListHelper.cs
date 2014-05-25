namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// The radio button list helper.
    /// </summary>
    public static class RadioButtonListHelper
    {
        /// <summary>
        /// The radio button list.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="listInfo">
        /// The list info.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<MvcHtmlString> RadioButtonList(
            this HtmlHelper helper,
            string name,
            IEnumerable<SelectListItem> listInfo)
        {
            return helper.RadioButtonList(name, listInfo, null);
        }

        /// <summary>
        /// The radio button list.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="listInfo">
        /// The list info.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<MvcHtmlString> RadioButtonList(
            this HtmlHelper helper,
            string name,
            IEnumerable<SelectListItem> listInfo,
            object htmlAttributes)
        {
            return helper.RadioButtonList(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        /// <summary>
        /// The radio button list.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="listInfo">
        /// The list info.
        /// </param>
        /// <param name="htmlAttributes">
        /// The html attributes.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static List<MvcHtmlString> RadioButtonList(
            this HtmlHelper helper,
            string name,
            IEnumerable<SelectListItem> listInfo,
            IDictionary<string, object> htmlAttributes)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The argument must have a value", "name");
            }

            if (listInfo == null)
            {
                throw new ArgumentNullException("listInfo");
            }

            var result = new List<MvcHtmlString>();

            foreach (SelectListItem info in listInfo)
            {
                result.Add(helper.InputElement(info, name, "radio", htmlAttributes));
            }

            return result;
        } 
    }
}