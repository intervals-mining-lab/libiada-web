using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace LibiadaWeb.Helpers
{
    public static class RazorExtensions
    {
        private static readonly String br = Environment.NewLine;

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
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            StringBuilder sb = new StringBuilder();

            foreach (SelectListItem info in listInfo)
            {
                sb.Append(helper.InputElement(info, name, "checkbox", htmlAttributes));
            }

            return MvcHtmlString.Create(sb.ToString());
        }


        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper helper, string name,
                                                       IEnumerable<SelectListItem> listInfo)
        {
            return helper.CheckBoxList(name, listInfo, null);
        }

        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper helper, string name,
                                                       IEnumerable<SelectListItem> listInfo, object htmlAttributes)
        {
            return helper.CheckBoxList(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        public static List<MvcHtmlString> CheckBoxList(this HtmlHelper helper, string name,
                                                       IEnumerable<SelectListItem> listInfo,
                                                       IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
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

        public static List<MvcHtmlString> RadioButtonList(this HtmlHelper helper, string name,
                                                          IEnumerable<SelectListItem> listInfo)
        {
            return helper.RadioButtonList(name, listInfo, null);
        }

        public static List<MvcHtmlString> RadioButtonList(this HtmlHelper helper, string name,
                                                          IEnumerable<SelectListItem> listInfo,
                                                          object htmlAttributes)
        {
            return helper.RadioButtonList(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        public static List<MvcHtmlString> RadioButtonList(this HtmlHelper helper, string name,
                                                          IEnumerable<SelectListItem> listInfo,
                                                          IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");

            List<MvcHtmlString> result = new List<MvcHtmlString>();

            foreach (SelectListItem info in listInfo)
            {
                result.Add(helper.InputElement(info, name, "radio", htmlAttributes));
            }

            return result;
        }

        public static MvcHtmlString InputElement(this HtmlHelper helper, SelectListItem info, String name,
                                                 String type, IDictionary<string, object> htmlAttributes)
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
        /// <param name="helper"></param>
        /// <param name="name">Имя и идентификатор</param>
        /// <param name="label">Текст рядом с галочкой</param>
        /// <param name="htmlAttributes">дополнительные аттрибуты</param>
        /// <returns></returns>
        public static MvcHtmlString CheckBox(this HtmlHelper helper, String name, String label,
                                             IDictionary<string, object> htmlAttributes)
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

            return
                MvcHtmlString.Create(elementBuilder.ToString(TagRenderMode.Normal) +
                                     hiddenElement.ToString(TagRenderMode.Normal) + "<br />");
        }

        public static MvcHtmlString MattersTable(this HtmlHelper helper, IEnumerable<SelectListItem> listInfo,
                                                 List<matter> matters)
        {
            List<MvcHtmlString> checkBoxes = helper.CheckBoxList("matterIds", listInfo);

            List<String> headers = new List<String> { "Название", "Описание", "Природа", "id удалённой БД" };

            List<List<String>> bodyData = new List<List<String>>();

            for (int i = 0; i < checkBoxes.Count; i++)
            {
                bodyData.Add(new List<String>());

                bodyData[i].Add(checkBoxes[i].ToString());
                bodyData[i].Add(matters[i].description);
                bodyData[i].Add(matters[i].nature.name);
                bodyData[i].Add(matters[i].id_in_remote_db);
            }

            return helper.Table(headers, bodyData);
        }

        public static MvcHtmlString Table(this HtmlHelper helper, List<String> headers,
                                          List<List<String>> data)
        {
            TagBuilder header = new TagBuilder("thead")
                {
                    InnerHtml = TableRow(headers, true)
                };

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < data.Count; i++)
            {
                builder.Append(TableRow(data[i], false));
            }

            TagBuilder body = new TagBuilder("tbody")
                {
                    InnerHtml = builder.ToString()
                };

            TagBuilder table = new TagBuilder("table")
                {
                    InnerHtml = br + header + br + body + br
                };
            
            return MvcHtmlString.Create(br + table + br);
        }

        private static String TableRow(List<String> data, bool header)
        {
            String cellType = header ? "th" : "td";

            StringBuilder builder = new StringBuilder(br);

            for (int i = 0; i < data.Count; i++)
            {
                TagBuilder dataCell = new TagBuilder(cellType) {InnerHtml = data[i] };
                builder.AppendLine(dataCell.ToString());
            }

            TagBuilder dataRow = new TagBuilder("tr") { InnerHtml = builder.ToString() };

            return br + dataRow + br;
        }
    }
}