using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace LibiadaWeb.Helpers
{
    public static class RazorExtensions
    {
        private static readonly String Br = Environment.NewLine;

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
            TagBuilder inputElement = new TagBuilder("input");
            if (info.Selected)
            {
                inputElement.MergeAttribute("checked", "checked");
            }
            inputElement.MergeAttributes(htmlAttributes);
            inputElement.MergeAttribute("type", type);
            inputElement.MergeAttribute("value", info.Value);
            inputElement.MergeAttribute("name", name);
            inputElement.InnerHtml = info.Text;

            return MvcHtmlString.Create(inputElement.ToString(TagRenderMode.Normal) + Br);
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
            TagBuilder checkBoxElement = new TagBuilder("input");
            checkBoxElement.MergeAttributes(htmlAttributes);
            checkBoxElement.MergeAttribute("type", "checkbox");
            checkBoxElement.MergeAttribute("value", "true");
            checkBoxElement.MergeAttribute("name", name);
            checkBoxElement.MergeAttribute("id", name);

            TagBuilder labelElement = new TagBuilder("label");
            labelElement.MergeAttribute("for", name);
            labelElement.InnerHtml = label;

            TagBuilder hiddenElement = new TagBuilder("input");
            hiddenElement.MergeAttribute("type", "hidden");
            hiddenElement.MergeAttribute("value", "false");
            hiddenElement.MergeAttribute("name", name);

            return
                MvcHtmlString.Create(checkBoxElement.ToString(TagRenderMode.Normal) + labelElement.ToString(TagRenderMode.Normal) +
                                     hiddenElement.ToString(TagRenderMode.Normal) + Br);
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

        public static MvcHtmlString ChainsTable(this HtmlHelper helper, IEnumerable<SelectListItem> listInfo,
                                                 List<chain> chains, List<String> languages, List<String> fastaHeaders)
        {
            List<MvcHtmlString> checkBoxes = helper.CheckBoxList("matterIds", listInfo);

            List<String> headers = new List<String> { "Название", "Форма записи", "Дата создания", "Тип фрагмента", "Позиция фрагмента", "Язык", "Заголовок fasta файла" };

            List<List<String>> bodyData = new List<List<String>>();

            for (int i = 0; i < checkBoxes.Count; i++)
            {
                bodyData.Add(new List<String>());

                bodyData[i].Add(checkBoxes[i].ToString());
                bodyData[i].Add(chains[i].notation.name);
                bodyData[i].Add(chains[i].creation_date.ToString());
                bodyData[i].Add(chains[i].piece_type.name);
                bodyData[i].Add(chains[i].piece_position.ToString());
                bodyData[i].Add(languages[i]);
                bodyData[i].Add(fastaHeaders[i]);

            }

            return helper.Table(headers, bodyData);
        }

        public static MvcHtmlString Table(this HtmlHelper helper, IEnumerable<string> headers,
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
                    InnerHtml = Br + header + Br + body + Br
                };
            
            return MvcHtmlString.Create(Br + table + Br);
        }

        private static String TableRow(IEnumerable<string> cells, bool header)
        {
            String cellType = header ? "th" : "td";

            StringBuilder builder = new StringBuilder(Br);

            foreach (string cell in cells)
            {
                TagBuilder dataCell = new TagBuilder(cellType) {InnerHtml = cell };
                builder.AppendLine(dataCell.ToString());
            }

            TagBuilder result = new TagBuilder("tr") { InnerHtml = builder.ToString() };

            return Br + result + Br;
        }
    }
}