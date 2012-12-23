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
                sb.Append(htmlHelper.CreateInputElement(info, name, "checkbox", htmlAttributes));
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
                result.Add(htmlHelper.CreateInputElement(info, name, "checkbox", htmlAttributes));
            }

            return result;
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
                result.Add(htmlHelper.CreateInputElement(info, name, "radio", htmlAttributes));
            }

            return result;
        }

        public static MvcHtmlString CreateInputElement(this HtmlHelper htmlHelper, SelectListItem info, String name, String type, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder elementBuilder = new TagBuilder("input");
            if (info.Selected)
            {
                elementBuilder.MergeAttribute("checked", "checked");
            }
            elementBuilder.MergeAttributes<string, object>(htmlAttributes);
            elementBuilder.MergeAttribute("type", type);
            elementBuilder.MergeAttribute("value", info.Value);
            elementBuilder.MergeAttribute("name", name);
            elementBuilder.InnerHtml = info.Text;

            return MvcHtmlString.Create(elementBuilder.ToString(TagRenderMode.Normal) + "<br />");
        }

        public static MvcHtmlString CheckBox(this HtmlHelper htmlHelper, String name, String label, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder elementBuilder = new TagBuilder("input");
            elementBuilder.MergeAttributes<string, object>(htmlAttributes);
            elementBuilder.MergeAttribute("type", "checkbox");
            elementBuilder.MergeAttribute("value", "true");
            elementBuilder.MergeAttribute("name", name);
            elementBuilder.InnerHtml = label;

            TagBuilder hiddenElement = new TagBuilder("input");
            hiddenElement.MergeAttribute("type", "hidden");
            hiddenElement.MergeAttribute("value", "false");
            hiddenElement.MergeAttribute("name", name);

            return MvcHtmlString.Create(elementBuilder.ToString(TagRenderMode.Normal) + hiddenElement.ToString(TagRenderMode.Normal) + "<br />");
        }
    }
}