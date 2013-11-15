using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace LibiadaWeb.Helpers
{
    public static class CheckBoxGroupHelper
    {
        public static MvcHtmlString CheckBoxGroup(this HtmlHelper helper, string name,
                                                 IEnumerable<SelectListItem> listInfo)
        {
            return helper.CheckBoxGroup(name, listInfo, null);
        }

        public static MvcHtmlString CheckBoxGroup(this HtmlHelper helper, string name,
                                                  IEnumerable<SelectListItem> listInfo, object htmlAttributes)
        {
            return helper.CheckBoxGroup(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString CheckBoxGroup(this HtmlHelper helper, string name,
                                                  IEnumerable<SelectListItem> listInfo,
                                                  IDictionary<string, object> htmlAttributes)
        {
            List<MvcHtmlString> checkBoxList = helper.CheckBoxList(name, listInfo, htmlAttributes);

            StringBuilder sb = new StringBuilder();

            foreach (MvcHtmlString checkBox in checkBoxList)
            {
                sb.Append(checkBox);
            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}