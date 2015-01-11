namespace LibiadaWeb.Helpers
{
    using System;
    using System.Text;
    using System.Web;

    /// <summary>
    /// The file helper.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// The read file stream.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="encoding">
        /// The encoding.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if file is null or empty.
        /// </exception>
        public static string ReadFileStream(HttpPostedFileBase file, Encoding encoding)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new ArgumentNullException("file", "File is null or empty.");
            }

            var fileStream = file.InputStream;
            var input = new byte[fileStream.Length];

            fileStream.Read(input, 0, (int)fileStream.Length);

            return encoding.GetString(input);
        }
    }
}
