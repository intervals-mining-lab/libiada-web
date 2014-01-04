using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace LibiadaWeb.Helpers
{
    public static class TableHelper
    {
        private static readonly string Br = Environment.NewLine;

        public static MvcHtmlString MattersTable(this HtmlHelper helper, IEnumerable<SelectListItem> listInfo,
                                                List<matter> matters)
        {
            List<MvcHtmlString> checkBoxes = helper.CheckBoxList("matterIds", listInfo);

            var headers = new List<String> { "Название", "Описание", "Природа"};

            var bodyData = new List<List<String>>();

            for (int i = 0; i < checkBoxes.Count; i++)
            {
                bodyData.Add(new List<String>());

                bodyData[i].Add(checkBoxes[i].ToString());
                bodyData[i].Add(matters[i].description);
                bodyData[i].Add(matters[i].nature.name);
            }

            return helper.Table(headers, bodyData);
        }

        public static MvcHtmlString ChainsTable(this HtmlHelper helper, IEnumerable<SelectListItem> listInfo,
                                                 List<chain> chains, List<String> languages, List<String> fastaHeaders, List<String> remoteIds )
        {
            List<MvcHtmlString> checkBoxes = helper.CheckBoxList("matterIds", listInfo);

            var headers = new List<String>
                {
                    "Название", 
                    "Форма записи", 
                    "Дата создания", 
                    "Тип фрагмента", 
                    "Позиция фрагмента", 
                    "Язык", 
                    "Заголовок fasta файла",
                    "id удалённой БД"
                };

            var bodyData = new List<List<String>>();

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
                bodyData[i].Add(remoteIds[i]);

            }

            return helper.Table(headers, bodyData);
        }

        public static MvcHtmlString Table(this HtmlHelper helper, IEnumerable<string> headers,
                                          List<List<String>> data)
        {
            var header = new TagBuilder("thead")
            {
                InnerHtml = TableRow(headers, true)
            };

            var builder = new StringBuilder();

            for (int i = 0; i < data.Count; i++)
            {
                builder.Append(TableRow(data[i], false));
            }

            var body = new TagBuilder("tbody")
            {
                InnerHtml = builder.ToString()
            };

            var table = new TagBuilder("table")
            {
                InnerHtml = Br + header + Br + body + Br
            };

            return MvcHtmlString.Create(Br + table + Br);
        }

        private static String TableRow(IEnumerable<string> cells, bool header)
        {
            var cellType = header ? "th" : "td";

            var builder = new StringBuilder(Br);

            foreach (string cell in cells)
            {
                var dataCell = new TagBuilder(cellType) { InnerHtml = cell };
                builder.AppendLine(dataCell.ToString());
            }

            var result = new TagBuilder("tr") { InnerHtml = builder.ToString() };

            return Br + result + Br;
        }
    }
}