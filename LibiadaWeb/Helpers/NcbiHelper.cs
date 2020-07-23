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
    using LibiadaWeb.Models.NcbiSequencesData;

    /// <summary>
    /// The ncbi helper.
    /// </summary>
    public static class NcbiHelper
    {
        /// <summary>
        /// The base url for all eutils.
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
        /// <param name="accession">
        /// Accession id of the sequence in ncbi (remote id).
        /// </param>
        /// <returns>
        /// The <see cref="List{FeatureItem}"/>.
        /// </returns>
        public static List<FeatureItem> GetFeatures(string accession)
        {
            GenBankMetadata metadata = GetMetadata(DownloadGenBankSequence(accession));
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
        /// <param name="acceccion">
        /// Accession id of the sequence in ncbi (remote id).
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream GetFastaFileStream(string acceccion)
        {
            return GetResponseStream(GetEfetchParamsString("fasta") + acceccion);
        }

        /// <summary>
        /// Extracts sequence from genbank file.
        /// </summary>
        /// <param name="accession">
        /// Accession id of the sequence in ncbi (remote id).
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static ISequence DownloadGenBankSequence(string accession)
        {
            ISequenceParser parser = new GenBankParser();
            string url = GetEfetchParamsString("gbwithparts") + accession;
            Stream dataStream = GetResponseStream(url);
            return parser.ParseOne(dataStream);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="includePartial"></param>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string[] GetIdsFromNcbiSearchResults(
            string data,
            bool includePartial,
            int minLength = 1,
            int maxLength = int.MaxValue)
        {
            string[] searchResults = Regex.Split(data, @"^\r\n", RegexOptions.Multiline);
            List<string> accessions = new List<string>();

            foreach (string block in searchResults)
            {
                if (!string.IsNullOrEmpty(block))
                {
                    string[] blockLines = block.Split('\n');
                    string seqenceName = blockLines[0];
                    string sequenceLength = blockLines[1];
                    string accession = blockLines[2];

                    if (includePartial || !seqenceName.Contains("partial"))
                    {
                        int length = GetLengthFromString(sequenceLength);
                        if (length >= minLength && length <= maxLength)
                        {
                            string[] idStrings = accession.Split(' ');
                            accessions.Add(idStrings[0]);
                        }
                    }
                }
            }
            return accessions.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="includePartial"></param>
        /// <returns></returns>
        public static List<NuccoreObject> SearchInNuccoreDb(string searchTerm, bool includePartial)
        {
            int retstart = 0;
            var urlEsearch = $"esearch.fcgi?db=nuccore&term={searchTerm}&usehistory=y&retmode=json";
            var esearchResponseString = GetResponceString(urlEsearch);
            ESearchResult eSearchResult = JsonConvert.DeserializeObject<ESearchResponce>(esearchResponseString).ESearchResult;
            var nuccoreObjects = new List<NuccoreObject>();

            int ElementCount;
            do
            {
                var urlEsummary = $"esummary.fcgi?db=nuccore" +
                                  $"&term={searchTerm}" +
                                  $"&usehistory=y&WebEnv={eSearchResult.NcbiWebEnvironment}" +
                                  $"&query_key=1&retmode=text&rettype=docsum&retmax=500&restart={retstart}";
                var esummaryResponse = GetResponceString(urlEsummary);
                retstart++;

                XmlSerializer serializer = new XmlSerializer(typeof(eSummaryResult));
                ElementCount = eSearchResult.Count;
                eSummaryResult DeserializeResult;
                using (TextReader reader = new StringReader(esummaryResponse))
                {
                    DeserializeResult = (eSummaryResult)serializer.Deserialize(reader);
                }

                foreach (ESummaryResultDocumentSummary element in DeserializeResult.DocumentSummary)
                {
                    if (includePartial || !element.Items[1].Value.Contains("partial"))
                    {
                        NuccoreObject nuccoreObject = new NuccoreObject();
                        nuccoreObject.Id = element.Id;
                        nuccoreObject.Name = element.Items[1].Value;
                        nuccoreObject.Accession = element.Items[0].Value;
                        nuccoreObject.Length = element.Items[8].Value;
                        nuccoreObjects.Add(nuccoreObject);
                    }
                }
            } while (nuccoreObjects.Count < ElementCount);
            return nuccoreObjects;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string FormatNcbiSearchTerm(string searchTerm, int? minLength = null, int? maxLength = null)
        {
            if (minLength != null && maxLength != null)
            {
                searchTerm +=  $"[All Fields] AND (\"{minLength}\"[SLEN] : \"{maxLength}\"[SLEN])";
            }
              return searchTerm;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        private static int GetLengthFromString(string stringLength)
        {
            stringLength = stringLength.Split(' ')[0];
            IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
            return int.Parse(stringLength, NumberStyles.Integer | NumberStyles.AllowThousands, provider);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string GetResponceString(string url)
        {
            var response = GetResponseStream(url);
            StreamReader reader = new StreamReader(response);
            string responseText = reader.ReadToEnd();

            return responseText;
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
    }
}
