using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace LibiadaWeb.Helpers
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
                sb.Append(CheckBox(info, name, htmlAttributes));
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
                 result.Add(MvcHtmlString.Create(CheckBox(info, name, htmlAttributes)));
            }

            return result;
        }

        public static String CheckBox(SelectListItem info, String name, IDictionary<string, object> htmlAttributes)
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
            return builder.ToString(TagRenderMode.Normal) + "<br />";
        }

        public static List<MvcHtmlString> RadioButtonGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo)
        {
            return htmlHelper.RadioButtonGroup(name, listInfo, ((IDictionary<string, object>)null));
        }

        public static List<MvcHtmlString> RadioButtonGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          object htmlAttributes)
        {
            return htmlHelper.RadioButtonGroup(name, listInfo, ((IDictionary<string, object>)new RouteValueDictionary(htmlAttributes)));
        }

        public static List<MvcHtmlString> RadioButtonGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            List<MvcHtmlString> result = new List<MvcHtmlString>();

            foreach (SelectListItem info in listInfo)
            {
                result.Add(MvcHtmlString.Create(RadioButton(info, name, htmlAttributes)));
            }

            return result;
        }

        private static String RadioButton(SelectListItem info, String name, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder builder = new TagBuilder("input");
            if (info.Selected)
            {
                builder.MergeAttribute("checked", "checked");
            }
            builder.MergeAttributes<string, object>(htmlAttributes);
            builder.MergeAttribute("type", "radio");
            builder.MergeAttribute("value", info.Value);
            builder.MergeAttribute("name", name);
            builder.InnerHtml = info.Text;
            return builder.ToString(TagRenderMode.Normal) + "<br />";
        }
    }
}