using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace LibiadaWeb.Helpers
{
    public static class RazorExtensions
    {
        public static MvcHtmlString CheckBoxGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo)
        {
            return htmlHelper.CheckBoxGroup(name, listInfo, null);
        }

        public static MvcHtmlString CheckBoxGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          object htmlAttributes)
        {
            return htmlHelper.CheckBoxGroup(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString CheckBoxGroup(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            StringBuilder sb = new StringBuilder();

            foreach (SelectListItem info in listInfo)
            {
                sb.Append(htmlHelper.InputElement(info, name, "checkbox", htmlAttributes));
            }

            return MvcHtmlString.Create(sb.ToString());
        }


        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo)
        {
            return htmlHelper.CheckBoxList(name, listInfo, null);
        }

        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          object htmlAttributes)
        {
            return htmlHelper.CheckBoxList(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            List<MvcHtmlString> result = new List<MvcHtmlString>();

            foreach (SelectListItem info in listInfo)
            {
                result.Add(htmlHelper.InputElement(info, name, "checkbox", htmlAttributes));
            }

            return result;
        }

        public static List<MvcHtmlString> RadioButtonList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo)
        {
            return htmlHelper.RadioButtonList(name, listInfo, null);
        }

        public static List<MvcHtmlString> RadioButtonList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          object htmlAttributes)
        {
            return htmlHelper.RadioButtonList(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        public static List<MvcHtmlString> RadioButtonList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> listInfo,
                                          IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            List<MvcHtmlString> result = new List<MvcHtmlString>();

            foreach (SelectListItem info in listInfo)
            {
                result.Add(htmlHelper.InputElement(info, name, "radio", htmlAttributes));
            }

            return result;
        }

        public static MvcHtmlString InputElement(this HtmlHelper htmlHelper, SelectListItem info, String name, String type, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder elementBuilder = new TagBuilder("input");
            if (info.Selected)
            {
                elementBuilder.MergeAttribute("checked", "checked");
            }
            elementBuilder.MergeAttributes(htmlAttributes);
            elementBuilder.MergeAttribute("type", type);
            elementBuilder.MergeAttribute("value", info.Value);
            elementBuilder.MergeAttribute("name", name);
            elementBuilder.InnerHtml = info.Text;

            return MvcHtmlString.Create(elementBuilder.ToString(TagRenderMode.Normal) + "<br />");
        }

        /// <summary>
        /// Костыль чтобы чекбоксы правильно передавались в контроллер.
        /// без него при неотмеченном чекбоксе в контроллер придёт null и он умрёт.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="name">Имя и идентификатор</param>
        /// <param name="label">Текст рядом с галочкой</param>
        /// <param name="htmlAttributes">дополнительные аттрибуты</param>
        /// <returns></returns>
        public static MvcHtmlString CheckBox(this HtmlHelper htmlHelper, String name, String label, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder elementBuilder = new TagBuilder("input");
            elementBuilder.MergeAttributes(htmlAttributes);
            elementBuilder.MergeAttribute("type", "checkbox");
            elementBuilder.MergeAttribute("value", "true");
            elementBuilder.MergeAttribute("name", name);
            elementBuilder.MergeAttribute("id", name);
            elementBuilder.InnerHtml = label;

            TagBuilder hiddenElement = new TagBuilder("input");
            hiddenElement.MergeAttribute("type", "hidden");
            hiddenElement.MergeAttribute("value", "false");
            hiddenElement.MergeAttribute("name", name);

            return MvcHtmlString.Create(elementBuilder.ToString(TagRenderMode.Normal) + hiddenElement.ToString(TagRenderMode.Normal) + "<br />");
        }

        public static MvcHtmlString MattersTable(this HtmlHelper htmlHelper, List<SelectListItem> listInfo, List<matter> data)
        {
            List<MvcHtmlString> checkBoxes = htmlHelper.CheckBoxList("matterIds", listInfo);
            String br = Environment.NewLine;

            TagBuilder name = new TagBuilder("th") {InnerHtml = "Название" };
            TagBuilder description = new TagBuilder("th") { InnerHtml = "Описание" };
            TagBuilder id = new TagBuilder("th") { InnerHtml = "id" };
            TagBuilder headRow = new TagBuilder("tr") { InnerHtml = br + name + br + description + br + id + br };
            TagBuilder header = new TagBuilder("thead") { InnerHtml = br + headRow + br };

            TagBuilder body = new TagBuilder("tbody");

            for (int i = 0; i < checkBoxes.Count; i++)
            {

                TagBuilder nameCell = new TagBuilder("td") { InnerHtml = checkBoxes[i].ToString() };
                TagBuilder descriptionCell = new TagBuilder("td") { InnerHtml = data[i].description };
                TagBuilder idCell = new TagBuilder("td") { InnerHtml = data[i].id_in_remote_db };
                TagBuilder bodyRow = new TagBuilder("tr") { InnerHtml = br + nameCell + br + descriptionCell + br + idCell + br };
                body.InnerHtml += br + bodyRow + br;
            }

            TagBuilder table = new TagBuilder("table") { InnerHtml = br + header + br + body + br };

            return MvcHtmlString.Create(br + table + br);
        }
    }
}