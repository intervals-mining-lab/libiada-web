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
            var metadata = GetMetadata(GetGenBankSequence(id));
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
            var sequences = GetGenBankSequences(ids);
            for (int i = 0; i < sequences.Length; i++)
            {
                var metadata = GetMetadata(sequences[i]);
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
        /// The <see cref="ISequence[]"/>.
        /// </returns>
        public static ISequence[] GetGenBankSequences(string[] ids)
        {
            ISequenceParser parser = new GenBankParser();
            string url = GetEfetchParamsString("gbwithparts") + string.Join(",", ids);
            Stream fileStream = GetResponseStream(url);
            ISequence[] result = parser.Parse(fileStream).ToArray();
            fileStream.Dispose();

            return result;
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
        /// Thrown if metadata is abscent.
        /// </exception>
        public static GenBankMetadata GetMetadata(ISequence sequence)
        {
            GenBankMetadata metadata = sequence.Metadata["GenBank"] as GenBankMetadata;

            if (metadata == null)
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
            var result = fastaParser.ParseOne(fastaFileStream);
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
        private static ISequence GetGenBankSequence(string id)
        {
            Stream fileStream = GetResponseStream(GetEfetchParamsString("gbwithparts") + id);
            ISequenceParser parser = new GenBankParser();
            return parser.ParseOne(fileStream);
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
            return @"efetch.fcgi?db=nuccore&retmode=text&rettype=" + retType + "&id=";
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
            var resultUrl = BaseUrl + url;
            var downloader = new WebClient();
            var memoryStream = new MemoryStream();

            lock (SyncRoot)
            {
                if (DateTimeOffset.Now - lastRequestDateTime < new TimeSpan(0, 0, 0, 0, 500))
                {
                    Thread.Sleep(500);
                }

                using (var stream = downloader.OpenRead(resultUrl))
                {
                    if (stream == null)
                    {
                        throw new Exception("Response stream was null.");
                    }

                    stream.CopyTo(memoryStream);
                }

                lastRequestDateTime = DateTimeOffset.Now;
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
