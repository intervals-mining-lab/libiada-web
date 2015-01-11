namespace LibiadaWeb.Helpers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// The ncbi helper.
    /// </summary>
    public static class NcbiHelper
    {
        /// <summary>
        /// The base url.
        /// </summary>
        private const string BaseUrl = @"http://eutils.ncbi.nlm.nih.gov/entrez/eutils/";

        /// <summary>
        /// The get id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if recivied not one sequence.
        /// </exception>
        public static int GetId(string id)
        {
            var memoryStream = GetResponceStream(@"esearch.fcgi?db=nucleotide&term=" + id);
            var doc = new XmlDocument();

            try
            {
                doc.Load(memoryStream);
            }
            finally
            {
                memoryStream.Close();
            }
            
            XmlNodeList elemList = doc.GetElementsByTagName("Id");

            if (elemList.Count != 1)
            {
                throw new Exception(string.Format("Resieved not one id of sequence (ids count = {0}).", elemList.Count));
            }

            return int.Parse(elemList[0].InnerText);
        }

        /// <summary>
        /// The get file.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream GetFileStream(string id)
        {
            return GetResponceStream(@"efetch.fcgi?db=nuccore&rettype=fasta&retmode=text&id=" + id);
        }

        /// <summary>
        /// The get genes.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream GetGenesFileStream(string id)
        {
            return GetResponceStream(@"efetch.fcgi?db=nuccore&rettype=gbwithparts&retmode=text&id=" + id);
        }

        /// <summary>
        /// The get sequence string.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetSequenceString(string id)
        {
            Stream fileStream = GetFileStream(id);
            var input = new byte[fileStream.Length];

            fileStream.Read(input, 0, (int)fileStream.Length);
            return Encoding.ASCII.GetString(input);
        }

        /// <summary>
        /// The get responce.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if response stream is null.
        /// </exception>
        private static Stream GetResponceStream(string url)
        {
            var resultUrl = BaseUrl + url;
            var request = WebRequest.Create(resultUrl);
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            var memoryStream = new MemoryStream();

            if (stream == null)
            {
                throw new Exception("Response stream was null.");
            }

            try
            {
                stream.CopyTo(memoryStream);
            }
            finally
            {
                stream.Close();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
