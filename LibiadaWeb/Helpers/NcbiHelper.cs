namespace LibiadaWeb.Helpers
{
    using System;
    using System.IO;
    using System.Net;
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
            var idUrl = BaseUrl + @"esearch.fcgi?db=nucleotide&term=" + id;
            var idRequest = WebRequest.Create(idUrl);
            var response = idRequest.GetResponse();
            var stream = response.GetResponseStream();
            var memoryStream = new MemoryStream();
            var doc = new XmlDocument();
            try
            {
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;
                doc.Load(memoryStream);
            }
            finally
            {
                memoryStream.Close();
                stream.Close();
            }

            XmlNodeList elemList = doc.GetElementsByTagName("Id");

            if (elemList.Count != 1)
            {
                throw new Exception("Количество идентификаторов цепочек для заданного запроса не равно 1.");
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
        public static Stream GetFile(string id)
        {
            var fileUrl = BaseUrl + @"efetch.fcgi?db=nuccore&rettype=fasta&retmode=text&id=" + id;
            var fileRequest = WebRequest.Create(fileUrl);
            var fileResponse = fileRequest.GetResponse();
            var fileStream = fileResponse.GetResponseStream();

            var fileMemoryStream = new MemoryStream();
            fileStream.CopyTo(fileMemoryStream);
            fileStream.Close();
            fileMemoryStream.Position = 0;
            return fileMemoryStream;
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
        public static Stream GetGenes(string id)
        {
            var fileUrl = BaseUrl + @"efetch.fcgi?db=nuccore&rettype=gbwithparts&retmode=text&id=" + id;
            var fileRequest = WebRequest.Create(fileUrl);
            var fileResponse = fileRequest.GetResponse();
            var fileStream = fileResponse.GetResponseStream();

            var fileMemoryStream = new MemoryStream();
            fileStream.CopyTo(fileMemoryStream);
            fileStream.Close();
            fileMemoryStream.Position = 0;
            return fileMemoryStream;
        }
    }
}
