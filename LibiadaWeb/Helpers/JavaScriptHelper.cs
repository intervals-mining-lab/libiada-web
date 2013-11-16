using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LibiadaWeb.Helpers
{
    public static class JavaScriptHelper
    {
        public static MvcHtmlString FillOptionsList(this HtmlHelper helper, String paramName, IEnumerable<SelectListItem> array)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("var {0} = new Array();", paramName).AppendLine();

            foreach (var option in array)
            {
                builder.AppendFormat("{0}.push(CreateOption({1},'{2}'));",
                    paramName, option.Value, HttpUtility.JavaScriptStringEncode(option.Text)).AppendLine();
            }
            builder.AppendFormat("paramsList.{0} = {0};", paramName).AppendLine();
            return MvcHtmlString.Create(builder.ToString());
        }
    }
}