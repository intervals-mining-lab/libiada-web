namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using Bio;
    using Bio.IO;
    using Bio.IO.FastA;
    using Bio.IO.GenBank;

    using LibiadaCore.Extensions;

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
        /// Extracts features from genBank files downloaded from ncbi.
        /// </summary>
        /// <param name="ids">
        /// Accession ids of the sequences in ncbi (remote ids).
        /// </param>
        /// <returns>
        /// The <see cref="List{FeatureItem}"/>.
        /// </returns>
        public static List<FeatureItem>[] GetFeatures(string[] ids)
        {
            var result = new List<FeatureItem>[ids.Length];
            ISequence[] sequences = GetGenBankSequences(ids);
            for (int i = 0; i < sequences.Length; i++)
            {
                GenBankMetadata metadata = GetMetadata(sequences[i]);
                result[i] = metadata.Features.All;
            }

            return result;
        }

        /// <summary>
        /// Extracts sequences from genbank files.
        /// </summary>
        /// <param name="ids">
        /// The ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:ISequence[]"/>.
        /// </returns>
        public static ISequence[] GetGenBankSequences(string[] ids)
        {
            if (ids.Length == 0)
            {
                return new ISequence[0];
            }

            var result = new List<ISequence>(ids.Length);
            for (int i = 0; i < ids.Length; i += 25)
            {
                string[] idsPortion = ids.SubArray(i, Math.Min(25, ids.Length - i));
                try
                {
                    result.AddRange(DownloadGenBankSequences(idsPortion));
                }
                catch (Exception exception)
                {
                    // if some of the sequences failed to load
                    // try each one separately
                    foreach (string id in idsPortion)
                    {
                        try
                        {
                            result.Add(DownloadGenBankSequence(id));
                        }
                        catch (Exception anotherException)
                        {
                            result.Add(null);
                        }
                    }
                }
            }

            return result.ToArray();
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
        /// Downloads sequences from genbank by id.
        /// </summary>
        /// <param name="ids">
        /// Remote sequences ids.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{ISequence}"/>.
        /// </returns>
        private static IEnumerable<ISequence> DownloadGenBankSequences(string[] ids)
        {
            ISequenceParser parser = new GenBankParser();
            string url = GetEfetchParamsString("gbwithparts") + string.Join(",", ids);
            Stream fileStream = GetResponseStream(url);
            return parser.Parse(fileStream);
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
        private static ISequence DownloadGenBankSequence(string id)
        {
            return DownloadGenBankSequences(new[] { id }).Single();
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
