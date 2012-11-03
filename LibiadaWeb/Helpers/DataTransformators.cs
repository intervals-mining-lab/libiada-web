using System.Text;
using System.Web.Mvc;

namespace LibiadaWeb.Helpers
{
    public static class DataTransformators
    {
        public static MvcHtmlString LineBreaker(this HtmlHelper htmlHelper, string line, int breakingLength)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < line.Length; i += breakingLength)
            {
                int blockLength = (i + breakingLength >= line.Length) ? (line.Length - i) : breakingLength;
                result.Append(line.Substring(i, blockLength) + " ");
            }

            return MvcHtmlString.Create(result.ToString());
        }

        public static string CleanFastaFile(string file)
        {
            string[] splittedFile = file.Split('\0');
            file = "";

            for (int k = 0; k < splittedFile.Length; k++)
            {
                file += splittedFile[k];
            }

            splittedFile = file.Split('\t');
            file = "";

            for (int l = 0; l < splittedFile.Length; l++)
            {
                file += splittedFile[l];
            }

            return file;
        }
    }
}