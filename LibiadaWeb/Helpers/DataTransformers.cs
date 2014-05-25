// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTransformers.cs" company="">
//   
// </copyright>
// <summary>
//   The data transformers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Text;

namespace LibiadaWeb.Helpers
{
    /// <summary>
    /// The data transformers.
    /// </summary>
    public static class DataTransformers
    {
        /// <summary>
        /// The clean fasta file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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