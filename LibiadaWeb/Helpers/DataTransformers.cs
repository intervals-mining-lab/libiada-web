using System.Text;

namespace LibiadaWeb.Helpers
{
    public static class DataTransformers
    {
        public static string CleanFastaFile(string file)
        {
            string[] splittedFile = file.Split(new [] { '\0', '\t'});
            
            var result = new StringBuilder();

            foreach (string line in splittedFile)
            {
                result.Append(line);
            }

            return result.ToString();
        }
    }
}