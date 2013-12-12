using System;
using System.IO;
using System.Net;
using System.Xml;

namespace LibiadaWeb.Helpers
{
    public static class NcbiHelper
    {
        public const String baseUrl = @"http://eutils.ncbi.nlm.nih.gov/entrez/eutils/";

        public static long GetId(string id)
        {
            String idUrl = baseUrl + @"esearch.fcgi?db=nucleotide&term=" + id;
            var idRequest = WebRequest.Create(idUrl);
            var response = idRequest.GetResponse();
            var stream = response.GetResponseStream();
            var memoiryStream = new MemoryStream();
            var doc = new XmlDocument();
            try
            {
                stream.CopyTo(memoiryStream);
                memoiryStream.Position = 0;
                doc.Load(memoiryStream);

            }
            finally
            {
                memoiryStream.Close();
                stream.Close();
            }

            XmlNodeList elemList = doc.GetElementsByTagName("Id");

            if (elemList.Count != 1)
            {
                throw new Exception("Количество идентификаторов цепочек для заданного запроса не равно 1.");
            }
            return Convert.ToInt64(elemList[0].InnerText);
        }

        public static Stream GetFile(String externalId)
        {
            return GetFile(GetId(externalId));
        }

        public static Stream GetFile(long id)
        {
            
            String fileUrl = baseUrl + @"efetch.fcgi?db=nuccore&rettype=fasta&retmode=text&id=" + id;
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