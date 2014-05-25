namespace LibiadaWeb.Helpers
{
    using System.Collections.Generic;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// The check box group helper.
    /// </summary>
    public static class CheckBoxGroupHelper
    {
        /// <summary>
        /// The check box group.
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
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString CheckBoxGroup(
            this HtmlHelper helper,
            string name,
            IEnumerable<SelectListItem> listInfo)
        {
            return helper.CheckBoxGroup(name, listInfo, null);
        }

        /// <summary>
        /// The check box group.
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
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString CheckBoxGroup(
            this HtmlHelper helper,
            string name,
            IEnumerable<SelectListItem> listInfo,
            object htmlAttributes)
        {
            return helper.CheckBoxGroup(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        /// <summary>
        /// The check box group.
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
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString CheckBoxGroup(
            this HtmlHelper helper,
            string name,
            IEnumerable<SelectListItem> listInfo,
            IDictionary<string, object> htmlAttributes)
        {
            List<MvcHtmlString> checkBoxList = helper.CheckBoxList(name, listInfo, htmlAttributes);

            var sb = new StringBuilder();

            foreach (MvcHtmlString checkBox in checkBoxList)
            {
                sb.Append(checkBox);
            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}