namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using LibiadaCore.Core;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The chain check controller.
    /// </summary>
    public class SequenceCheckController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly CommonSequenceRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceCheckController"/> class.
        /// </summary>
        public SequenceCheckController() : base("SequenceCheck", "Sequence check")
        {
            db = new LibiadaWebEntities();
            chainRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.matterId = new SelectList(db.Matter, "id", "name");
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(long matterId, string[] file)
        {
            return Action(() =>
            {
                var myFile = Request.Files[0];

                if (myFile == null || myFile.ContentLength == 0)
                {
                    throw new ArgumentNullException("file", "Sequence file not found or empty.");
                }

                int fileLen = myFile.ContentLength;
                var input = new byte[fileLen];

                // Initialize the stream.
                var fileStream = myFile.InputStream;

                // Read the file into the byte array.
                fileStream.Read(input, 0, fileLen);

                // Copy the byte array into a string.
                string stringChain = Encoding.ASCII.GetString(input);
                string[] tempString = stringChain.Split('\n', '\r');

                var chainStringBuilder = new StringBuilder();
                string fastaHeader = tempString[0];

                for (int j = 1; j < tempString.Length; j++)
                {
                    chainStringBuilder.Append(tempString[j]);
                }

                string resultStringChain = DataTransformers.CleanFastaFile(chainStringBuilder.ToString());

                var libiadaChain = new BaseChain(resultStringChain);

                long chainId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;

                string message;

                if (!db.DnaSequence.Any(d => d.FastaHeader.Equals(fastaHeader)))
                {
                    message = "объекта с заголовком " + fastaHeader + " не существует";

                    return new Dictionary<string, object> { { "message", message } };
                }

                BaseChain dataBaseChain = chainRepository.ToLibiadaBaseChain(chainId);

                if (dataBaseChain.Equals(libiadaChain))
                {
                    message = "Цепочки в БД и в файле идентичны";
                }
                else
                {
                    if (libiadaChain.Alphabet.Cardinality != dataBaseChain.Alphabet.Cardinality)
                    {
                        message = "Размеры алфавитов не совпадают. В базе - " + dataBaseChain.Alphabet.Cardinality +
                                  ". В файле - " + libiadaChain.Alphabet.Cardinality;

                        return new Dictionary<string, object> { { "message", message } };
                    }

                    for (int i = 0; i < libiadaChain.Alphabet.Cardinality; i++)
                    {
                        if (!libiadaChain.Alphabet[i].ToString().Equals(dataBaseChain.Alphabet[i].ToString()))
                        {
                            message = i + "Элементы алфавитов не равны. В базе - " + dataBaseChain.Alphabet[i] +
                                      ". В файле - " + libiadaChain.Alphabet[i];

                            return new Dictionary<string, object> { { "message", message } };
                        }
                    }

                    if (libiadaChain.GetLength() != dataBaseChain.GetLength())
                    {
                        message = "Длина цепочки в базе " + dataBaseChain.GetLength() + ", а длина цепочки из файла " +
                                  libiadaChain.GetLength();

                        return new Dictionary<string, object> { { "message", message } };
                    }

                    int[] libiadaBuilding = libiadaChain.Building;
                    int[] dataBaseBuilding = dataBaseChain.Building;

                    for (int j = 0; j < libiadaChain.GetLength(); j++)
                    {
                        if (libiadaBuilding[j] != dataBaseBuilding[j])
                        {
                            message = j + "Элементы цепочек не совпадают. В базе " + dataBaseBuilding[j] + ". В файле " +
                                      libiadaBuilding[j];

                            return new Dictionary<string, object> { { "message", message } };
                        }
                    }

                    message = "Цепочки Шрёдингера - они равны и не равны одновременно";
                }

                return new Dictionary<string, object> { { "message", message } };
            });
        }
    }
}
