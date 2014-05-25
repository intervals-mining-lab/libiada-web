// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CheckBoxListHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The check box list helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace LibiadaWeb.Helpers
{
    /// <summary>
    /// The check box list helper.
    /// </summary>
    public static class CheckBoxListHelper
    {
        /// <summary>
        /// The check box list.
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
        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper helper, string name, 
                                                       IEnumerable<SelectListItem> listInfo)
        {
            return helper.CheckBoxList(name, listInfo, null);
        }

        /// <summary>
        /// The check box list.
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
        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper helper, string name, 
                                                       IEnumerable<SelectListItem> listInfo, object htmlAttributes)
        {
            return helper.CheckBoxList(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        /// <summary>
        /// The check box list.
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
        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper helper, string name, 
                                                       IEnumerable<SelectListItem> listInfo, 
                                                       IDictionary<string, object> htmlAttributes)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            List<MvcHtmlString> result = new List<MvcHtmlString>();

            foreach (SelectListItem info in listInfo)
            {
                result.Add(helper.InputElement(info, name, "checkbox", htmlAttributes));
            }

            return result;
        } 
    }
}