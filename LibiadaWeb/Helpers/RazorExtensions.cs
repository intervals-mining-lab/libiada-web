using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace IvtLibrary.Helpers
{
    public static class RazorExtensions
    {
        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo)
        {
            return htmlHelper.CheckBoxList(name, listInfo, ((IDictionary<string, object>)null));
        }

        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          object htmlAttributes)
        {
            return htmlHelper.CheckBoxList(name, listInfo, ((IDictionary<string, object>)new RouteValueDictionary(htmlAttributes)));
        }

        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            StringBuilder sb = new StringBuilder();

            foreach (SelectListItem info in listInfo)
            {
                TagBuilder builder = new TagBuilder("input");
                if (info.Selected)
                {
                    builder.MergeAttribute("checked", "checked");
                }
                builder.MergeAttributes<string, object>(htmlAttributes);
                builder.MergeAttribute("type", "checkbox");
                builder.MergeAttribute("value", info.Value);
                builder.MergeAttribute("name", name);
                builder.InnerHtml = info.Text;
                sb.Append(builder.ToString(TagRenderMode.Normal));
                sb.Append("<br />");
            }

            return MvcHtmlString.Create(sb.ToString());
        }


        public static List<MvcHtmlString> CheckBoxGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo)
        {
            return htmlHelper.CheckBoxGroup(name, listInfo, ((IDictionary<string, object>)null));
        }

        public static List<MvcHtmlString> CheckBoxGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          object htmlAttributes)
        {
            return htmlHelper.CheckBoxGroup(name, listInfo, ((IDictionary<string, object>)new RouteValueDictionary(htmlAttributes)));
        }

        public static List<MvcHtmlString> CheckBoxGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            List<MvcHtmlString> result = new List<MvcHtmlString>();

            foreach (SelectListItem info in listInfo)
            {
                StringBuilder sb = new StringBuilder();
                TagBuilder builder = new TagBuilder("input");
                if (info.Selected)
                {
                    builder.MergeAttribute("checked", "checked");
                }
                builder.MergeAttributes<string, object>(htmlAttributes);
                builder.MergeAttribute("type", "checkbox");
                builder.MergeAttribute("value", info.Value);
                builder.MergeAttribute("name", name);
                builder.InnerHtml = info.Text;
                sb.Append(builder.ToString(TagRenderMode.Normal));
                sb.Append("<br />");
                result.Add(MvcHtmlString.Create(sb.ToString()));
            }

            return result;
        }
    }
}