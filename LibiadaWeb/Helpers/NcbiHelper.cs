﻿namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;

    using Bio;
    using Bio.IO;
    using Bio.IO.FastA;
    using Bio.IO.GenBank;

    using Newtonsoft.Json;

    using LibiadaWeb.Models.NcbiSequencesData;
    using Newtonsoft.Json.Linq;
    using System.Configuration;


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

        private static readonly string ApiKey = ConfigurationManager.AppSettings["NcbiApiKey"];

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
        /// <param name="accession">
        /// Accession id of the sequence in ncbi (remote id).
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream GetFastaFileStream(string accession)
        {
            string url = GetEfetchParamsString("fasta", accession);
            return GetResponseStream(url);
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
            string url = GetEfetchParamsString("gbwithparts", accession);
            Stream dataStream = GetResponseStream(url);
            return parser.ParseOne(dataStream);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data">
        /// 
        /// </param>
        /// <param name="includePartial">
        /// 
        /// </param>
        /// <param name="minLength">
        /// 
        /// </param>
        /// <param name="maxLength">
        /// 
        /// </param>
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
        /// <param name="searchTerm">
        /// 
        /// </param>
        /// <param name="includePartial">
        /// 
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        public static List<NuccoreObject> ExecuteESummaryRequest(string searchTerm, bool includePartial)
        {
            (string ncbiWebEnvironment, string queryKey) = ExecuteESearchRequest(searchTerm);
            return ExecuteESummaryRequest(ncbiWebEnvironment, queryKey, includePartial);
        }

        public static List<NuccoreObject> ExecuteESummaryRequest(string ncbiWebEnvironment, string queryKey, bool includePartial)
        {
            var nuccoreObjects = new List<NuccoreObject>();
            const short retmax = 500;
            int retstart = 0;
            var eSummaryResults = new List<ESummaryResult>();

            do
            {
                var urlEsummary = $"esummary.fcgi?db=nuccore&retmode=json" +
                                  $"&WebEnv={ncbiWebEnvironment}" +
                                  $"&query_key={queryKey}" +
                                  $"&retmax={retmax}&retstart={retstart}";
                var esummaryResponse = GetResponceString(urlEsummary);
                retstart += retmax;

                var esummaryResultJObject = JObject.Parse(esummaryResponse)["result"];
                if (esummaryResultJObject != null)
                {
                    // removing array of uids because it breakes deserialization
                    eSummaryResults = esummaryResultJObject
                                            .Children<JProperty>()
                                            .Where(j => j.Name != "uids")
                                            .Select(j => j.Value.ToObject<ESummaryResult>())
                                            .ToList();

                    // alternative implementation
                    //((JObject)esummaryResultJObject).Remove("uids");
                    //var eSummaryResults = JsonConvert.DeserializeObject<Dictionary<string, ESummaryResult>>(esummaryResultJObject.ToString()).Values;

                    foreach (ESummaryResult result in eSummaryResults)
                    {
                        bool isPartial = result.Title.Contains("partial") || string.IsNullOrEmpty(result.Completeness);
                        if (includePartial || !isPartial)
                        {
                            NuccoreObject nuccoreObject = new NuccoreObject
                            {
                                Title = result.Title,
                                Organism = result.Organism,
                                AccessionVersion = result.AccessionVersion,
                                Completeness = result.Completeness,
                                UpdateDate = result.UpdateDate
                            };
                            nuccoreObjects.Add(nuccoreObject);
                        }
                    }
                }
            } while (eSummaryResults.Count == retmax);

            return nuccoreObjects;
        }

        /// <summary>
        /// Requests web environment and query key from ncbi for given seqrch query.
        /// </summary>
        /// <param name="searchTerm">
        /// Search term of first part of search term.
        /// </param>
        /// <returns>
        /// Tuple of NcbiWebEnvironment and QueryKey <see cref="string"/>s.
        /// </returns>
        public static (string, string) ExecuteESearchRequest(string searchTerm)
        {
            var urlEsearch = $"esearch.fcgi?db=nuccore&term={searchTerm}&usehistory=y&retmode=json";
            var esearchResponseString = GetResponceString(urlEsearch);
            ESearchResult eSearchResult = JsonConvert.DeserializeObject<ESearchResponce>(esearchResponseString).ESearchResult;
            return (eSearchResult.NcbiWebEnvironment, eSearchResult.QueryKey);
        }

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static (string, string) ExecuteEPostRequest(string ids)
        {
            var urlEPost = $"epost.fcgi";
            string requestResult;
            using (var webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                Uri url = new Uri(BaseUrl + urlEPost);

                string data = $"db=nuccore&id={ids}";

                // adding api key to request if there is any
                if (!string.IsNullOrEmpty(ApiKey))
                {
                    data += $"&api_key={ApiKey}";
                }

                requestResult = webClient.UploadString(url, data);
            }

            XmlDocument xmlReader = new XmlDocument();
            xmlReader.LoadXml(requestResult);
            var result = xmlReader.LastChild;
            return (result["WebEnv"].FirstChild.Value, result["QueryKey"].FirstChild.Value);
        }

        /// <summary>
        /// Adds length linitations on sought sequences if any provided.
        /// </summary>
        /// <param name="searchTerm">
        /// Search term for all fields.
        /// </param>
        /// <param name="minLength">
        /// Minimal sequence length.
        /// </param>
        /// <param name="maxLength">
        /// Maximum sequence length.
        /// </param>
        /// <returns>
        /// Search term as is or formatet seqrch term with length limitations.
        /// </returns>
        public static string FormatNcbiSearchTerm(string searchTerm, int? minLength = null, int? maxLength = null)
        {
            if (minLength == null && maxLength == null)
            {
                return $"\"{searchTerm}\"[Organism]";
            }

            return $"\"{searchTerm}\"[Organism] AND (\"{minLength ?? 1}\"[SLEN] : \"{maxLength ?? int.MaxValue}\"[SLEN])";
        }

        /// <summary>
        /// Converts string formated in en-US culture into integer.
        /// </summary>
        /// <param name="integer">
        ///  String containing integer in american format.
        /// </param>
        /// <returns>
        /// Integer value.
        /// </returns>
        /// <example>
        /// (string)"1,111,520" => (int)1111520
        /// </example>
        private static int GetLengthFromString(string integer)
        {
            integer = integer.Split(' ')[0];
            IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
            return int.Parse(integer, NumberStyles.Integer | NumberStyles.AllowThousands, provider);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="url">
        /// 
        /// </param>
        /// <returns>
        /// 
        /// </returns>
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
        /// Response returned type.
        /// </param>
        /// <param name="accessions">
        /// Sequences acessions in genBank.
        /// </param>
        /// <returns>
        /// efetch part of url with params as <see cref="string"/>.
        /// </returns>
        private static string GetEfetchParamsString(string retType, string accessions)
        {
            return $"efetch.fcgi?db=nuccore&retmode=text&rettype={retType}&id={accessions}";
        }

        /// <summary>
        /// Downloads response from base url with given params.
        /// </summary>
        /// <param name="url">
        /// The params url (without base url).
        /// </param>
        /// <returns>
        /// The response as <see cref="Stream"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if response stream is null.
        /// </exception>
        private static Stream GetResponseStream(string url)
        {
            // adding api key to request if there is any
            if (!string.IsNullOrEmpty(ApiKey))
            {
                url += $"&api_key={ApiKey}";
            }

            string resultUrl = BaseUrl + url;

            var memoryStream = new MemoryStream();

            lock (SyncRoot)
            {
                WaitForRequest();
                using (var downloader = new WebClient())
                {
                    using (Stream stream = downloader.OpenRead(resultUrl))
                    {
                        if (stream == null)
                        {
                            throw new Exception("Response stream was null.");
                        }

                        stream.CopyTo(memoryStream);
                    }
                }
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// NCBI allows only 3 (10 for registered users) requests per second.
        /// So we wait between requests to be sure.
        /// </summary>
        private static void WaitForRequest()
        {
            int delay = string.IsNullOrEmpty(ApiKey) ? 334 : 100;

            if (DateTimeOffset.Now - lastRequestDateTime < new TimeSpan(0, 0, 0, 0, delay))
            {
                Thread.Sleep(delay);
            }

            lastRequestDateTime = DateTimeOffset.Now;
        }
    }
}
