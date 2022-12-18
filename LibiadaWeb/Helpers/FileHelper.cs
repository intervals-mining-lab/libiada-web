﻿namespace LibiadaWeb.Helpers
{
    using System;
    using System.IO;
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
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if file is null or empty.
        /// </exception>
        public static Stream GetFileStream(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new ArgumentNullException(nameof(file), "File is null or empty.");
            }

            return file.InputStream;
        }

        /// <summary>
        /// The read sequence from stream.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ReadSequenceFromStream(Stream stream)
        {
            var input = new byte[stream.Length];
            stream.Read(input, 0, (int)stream.Length);
            stream.Dispose();
            return Encoding.UTF8.GetString(input);
        }
    }
}
