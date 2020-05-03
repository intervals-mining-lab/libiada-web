using System.Text.RegularExpressions;
using Accord.Math;

namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;

    using Bio;
    using Bio.IO;
    using Bio.IO.FastA;
    using Bio.IO.GenBank;

    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using LibiadaWeb.Models.SequencesData;

    /// <summary>
    /// The ncbi helper.
    /// </summary>
    public static class NcbiHelper
    {
        /// <summary>
        /// The base url.
        /// </summary>
        private const string BaseUrl = @"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/";

        /// <summary>
        /// Synchronization object.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The last request date time.
        /// </summary>
        private static DateTimeOffset lastRequestDateTime = DateTimeOffset.MinValue;

        /// <summary>
        /// Extracts features from genBank file downloaded from ncbi.
        /// </summary>
        /// <param name="id">
        /// Accession id of the sequence in ncbi (remote id).
        /// </param>
        /// <returns>
        /// The <see cref="List{FeatureItem}"/>.
        /// </returns>
        public static List<FeatureItem> GetFeatures(string id)
        {
            GenBankMetadata metadata = GetMetadata(DownloadGenBankSequence(id));
            return metadata.Features.All;
        }

        /// <summary>
        /// Extracts metadata from genbank file.
        /// </summary>
        /// <param name="sequence">
        /// Sequence extracted from genbank file.
        /// </param>
        /// <returns>
        /// The <see cref="GenBankMetadata"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if metadata is absent.
        /// </exception>
        public static GenBankMetadata GetMetadata(ISequence sequence)
        {
            if (!(sequence.Metadata["GenBank"] is GenBankMetadata metadata))
            {
                throw new Exception("GenBank file metadata is empty.");
            }

            return metadata;
        }

        /// <summary>
        /// Downloads sequence as fasta file from ncbi.
        /// </summary>
        /// <param name="fastaFileStream">
        /// The fasta file stream.
        /// </param>
        /// <returns>
        /// The <see cref="ISequence"/>.
        /// </returns>
        public static ISequence GetFastaSequence(Stream fastaFileStream)
        {
            var fastaParser = new FastAParser();
            ISequence result = fastaParser.ParseOne(fastaFileStream);
            fastaFileStream.Dispose();
            return result;
        }

        /// <summary>
        /// The get file.
        /// </summary>
        /// <param name="id">
        /// Accession id of the sequence in ncbi (remote id).
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream GetFastaFileStream(string id)
        {
            return GetResponseStream(GetEfetchParamsString("fasta") + id);
        }

        /// <summary>
        /// Extracts sequence from genbank file.
        /// </summary>
        /// <param name="id">
        /// Accession id of the sequence in ncbi (remote id).
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static ISequence DownloadGenBankSequence(string id)
        {
            ISequenceParser parser = new GenBankParser();
            string url = GetEfetchParamsString("gbwithparts") + id;
            Stream dataStream = GetResponseStream(url);
            return parser.ParseOne(dataStream);
        }

        /// <summary>
        /// Creates efetch params string with given return type.
        /// </summary>
        /// <param name="retType">
        /// The return type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetEfetchParamsString(string retType)
        {
            return $"efetch.fcgi?db=nuccore&retmode=text&rettype={retType}&id=";
        }

        /// <summary>
        /// The get response.
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
        private static Stream GetResponseStream(string url)
        {
            string resultUrl = BaseUrl + url;
            var downloader = new WebClient();
            var memoryStream = new MemoryStream();

            lock (SyncRoot)
            {
                WaitForRequest();

                using (Stream stream = downloader.OpenRead(resultUrl))
                {
                    if (stream == null)
                    {
                        throw new Exception("Response stream was null.");
                    }

                    stream.CopyTo(memoryStream);
                }
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// NCBI allows only 3 requests per second.
        /// So we wait half a second between requests to be sure.
        /// </summary>
        private static void WaitForRequest()
        {
            if (DateTimeOffset.Now - lastRequestDateTime < new TimeSpan(0, 0, 0, 0, 500))
            {
                Thread.Sleep(500);
            }

            lastRequestDateTime = DateTimeOffset.Now;
        }

        public static string[] GetIdFromFile(
            string data,
            bool includePartial,
            int minLength = 1,
            int maxLength = int.MaxValue)
        {
            string[] fullText = Regex.Split(data, @"^\r\n", RegexOptions.Multiline);
            List<string> idList = new List<string>();
            int gensName = 0;
            int numberStrWithId = 2;
            int firstId = 0;
            foreach (var block in fullText)
            {
                if (!string.IsNullOrEmpty(block))
                {
                    string[] stringsInBlock = block.Split('\n');
                    if (includePartial || !stringsInBlock[gensName].Contains("partial"))
                    {
                        int length = GetLengthFromString(stringsInBlock[1]);
                        if (length >= minLength && length <= maxLength)
                        {
                            string[] idStrings = stringsInBlock[numberStrWithId].Split(' ');
                            idList.Add(idStrings[firstId]);
                        }
                    }
                }
            }
            return idList.ToArray();
        }

        private static int GetLengthFromString(string stringLength)
        {
            stringLength = stringLength.Split(' ')[0];
            IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
            int length = int.Parse(stringLength, NumberStyles.Integer |
                NumberStyles.AllowThousands, provider);
            return length;
        }
        public static List<NuccoreObject> DeserializeQueryObject(string DataBaseName, string searchTerm)
        {
            int retstart = 0;
            var urlEsearch = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=" +
                DataBaseName + "&term=" + searchTerm + "&usehistory=y&retmode=json";
            var esearchResponse = GetAnswer(urlEsearch);
            ESearchResult WebEnv = JsonConvert.DeserializeObject<ESearchResult>(esearchResponse);
            List<NuccoreObject> nuccoreObjects = new List<NuccoreObject>();

            int ElementCount;
            do
            {
                var urlEsummary = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esummary.fcgi?db="
                    + DataBaseName + "&term=" + searchTerm + "&usehistory=y&WebEnv=" + WebEnv.Response.WebEnvironment
                    + "&query_key=1&retmode=text&rettype=docsum&retmax=500&restart=" + retstart;//Paste ur url here  
                var esummaryResponse = GetAnswer(urlEsummary);
                //      Console.WriteLine(answer);
                XmlSerializer serializer = new XmlSerializer(typeof(eSummaryResult));
                ElementCount = WebEnv.Response.Count;
                eSummaryResult DeserializeResult;
                using (TextReader reader = new StringReader(esummaryResponse))
                {
                    DeserializeResult = (eSummaryResult)serializer.Deserialize(reader);
                }

                foreach (var element in DeserializeResult.DocSum)
                {
                    NuccoreObject nuccoreObject = new NuccoreObject();
                    nuccoreObject.Id = element.Id;
                    nuccoreObject.Name = element.Item[1].Value;
                    nuccoreObject.Accession = element.Item[0].Value;
                    nuccoreObject.Length = element.Item[8].Value;
                    nuccoreObjects.Add(nuccoreObject);
                }
                retstart++;
            } while (nuccoreObjects.Count < ElementCount);
            return nuccoreObjects;
        }
            
        

        public static string FormatTermString(string searchTerm, int? minLength = null, int? maxLength = null)
        {
            if (minLength != null && maxLength != null)
            {
                searchTerm +=  $"[All Fields] AND (\"{minLength}\"[SLEN] : \"{maxLength}\"[SLEN])";
            }
              return searchTerm; 
        }
        public static string GetAnswer(string url)
        {
            WebRequest request = WebRequest.Create(url);

            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());

            string responseText = reader.ReadToEnd();

            return responseText;
        }
    }
}
