namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web.Mvc;

    /// <summary>
    /// The table helper.
    /// </summary>
    public static class TableHelper
    {
        /// <summary>
        /// The br.
        /// </summary>
        private static readonly string Br = Environment.NewLine;

        /// <summary>
        /// The matters table.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="listInfo">
        /// The list info.
        /// </param>
        /// <param name="matters">
        /// The matters.
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString MattersTable(
            this HtmlHelper helper,
            IEnumerable<SelectListItem> listInfo,
            List<Matter> matters)
        {
            List<MvcHtmlString> checkBoxes = helper.CheckBoxList("matterIds", listInfo);

            var headers = new List<string> { "Name", "Description", "Nature" };

            var bodyData = new List<List<string>>();

            for (int i = 0; i < checkBoxes.Count; i++)
            {
                bodyData.Add(new List<string>());

                bodyData[i].Add(checkBoxes[i].ToString());
                bodyData[i].Add(matters[i].Description);
                bodyData[i].Add(matters[i].Nature.Name);
            }

            return helper.Table(headers, bodyData);
        }

        /// <summary>
        /// The sequences table.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="listInfo">
        /// The list info.
        /// </param>
        /// <param name="commonSequences">
        /// The sequences.
        /// </param>
        /// <param name="languages">
        /// The languages.
        /// </param>
        /// <param name="fastaHeaders">
        /// The fasta headers.
        /// </param>
        /// <param name="remoteIds">
        /// The remote ids.
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString SequencesTable(
            this HtmlHelper helper,
            IEnumerable<SelectListItem> listInfo,
            List<CommonSequence> commonSequences,
            List<string> languages,
            List<string> fastaHeaders,
            List<string> remoteIds)
        {
            List<MvcHtmlString> checkBoxes = helper.CheckBoxList("matterIds", listInfo);

            var headers = new List<string>
                {
                    "Name", 
                    "Notation", 
                    "Created", 
                    "Piece type", 
                    "Position", 
                    "Language", 
                    "Fasta file header", 
                    "Remote id"
                };

            var bodyData = new List<List<string>>();

            for (int i = 0; i < checkBoxes.Count; i++)
            {
                bodyData.Add(new List<string>());

                bodyData[i].Add(checkBoxes[i].ToString());
                bodyData[i].Add(commonSequences[i].Notation.Name);
                bodyData[i].Add(commonSequences[i].Created.ToString());
                bodyData[i].Add(commonSequences[i].PieceType.Name);
                bodyData[i].Add(commonSequences[i].PiecePosition.ToString());
                bodyData[i].Add(languages[i]);
                bodyData[i].Add(fastaHeaders[i]);
                bodyData[i].Add(remoteIds[i]);
            }

            return helper.Table(headers, bodyData);
        }

        /// <summary>
        /// The table.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString Table(this HtmlHelper helper, IEnumerable<string> headers, List<List<string>> data)
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

            table.AddCssClass("table");
            return MvcHtmlString.Create(Br + table + Br);
        }

        /// <summary>
        /// The table row.
        /// </summary>
        /// <param name="cells">
        /// The cells.
        /// </param>
        /// <param name="header">
        /// The header.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string TableRow(IEnumerable<string> cells, bool header)
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
