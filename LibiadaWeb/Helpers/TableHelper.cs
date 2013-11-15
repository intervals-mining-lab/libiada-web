using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace LibiadaWeb.Helpers
{
    public static class TableHelper
    {
        private static string Br = Environment.NewLine;

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
                TagBuilder dataCell = new TagBuilder(cellType) { InnerHtml = cell };
                builder.AppendLine(dataCell.ToString());
            }

            TagBuilder result = new TagBuilder("tr") { InnerHtml = builder.ToString() };

            return Br + result + Br;
        }
    }
}