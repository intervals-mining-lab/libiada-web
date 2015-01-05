namespace LibiadaWeb.Helpers
{
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// The java script helper.
    /// </summary>
    public static class JavaScriptHelper
    {
        /// <summary>
        /// The fill options list.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="paramName">
        /// The param name.
        /// </param>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString FillOptionsList(this HtmlHelper helper, string paramName, IEnumerable<SelectListItem> array)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("var {0} = new Array();", paramName).AppendLine();

            foreach (var option in array)
            {
                builder.AppendFormat("{0}.push(CreateOption({1},'{2}'));", paramName, option.Value, HttpUtility.JavaScriptStringEncode(option.Text)).AppendLine();
            }

            builder.AppendFormat("paramsList.{0} = {0};", paramName).AppendLine();
            return MvcHtmlString.Create(builder.ToString());
        }
    }
}
