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
    /// The sequence check controller.
    /// </summary>
    public class SequenceCheckController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceCheckController"/> class.
        /// </summary>
        public SequenceCheckController() : base("SequenceCheck", "Sequence check")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
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
                string stringSequence = Encoding.ASCII.GetString(input);
                string[] tempString = stringSequence.Split('\n', '\r');

                var sequenceStringBuilder = new StringBuilder();
                string fastaHeader = tempString[0];

                for (int j = 1; j < tempString.Length; j++)
                {
                    sequenceStringBuilder.Append(tempString[j]);
                }

                string resultStringSequence = DataTransformers.CleanFastaFile(sequenceStringBuilder.ToString());

                var chain = new BaseChain(resultStringSequence);

                long sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;

                string message;

                if (!db.DnaSequence.Any(d => d.FastaHeader.Equals(fastaHeader)))
                {
                    message = "объекта с заголовком " + fastaHeader + " не существует";

                    return new Dictionary<string, object> { { "message", message } };
                }

                BaseChain dataBaseChain = commonSequenceRepository.ToLibiadaBaseChain(sequenceId);

                if (dataBaseChain.Equals(chain))
                {
                    message = "Цепочки в БД и в файле идентичны";
                }
                else
                {
                    if (chain.Alphabet.Cardinality != dataBaseChain.Alphabet.Cardinality)
                    {
                        message = "Размеры алфавитов не совпадают. В базе - " + dataBaseChain.Alphabet.Cardinality +
                                  ". В файле - " + chain.Alphabet.Cardinality;

                        return new Dictionary<string, object> { { "message", message } };
                    }

                    for (int i = 0; i < chain.Alphabet.Cardinality; i++)
                    {
                        if (!chain.Alphabet[i].ToString().Equals(dataBaseChain.Alphabet[i].ToString()))
                        {
                            message = i + "Элементы алфавитов не равны. В базе - " + dataBaseChain.Alphabet[i] +
                                      ". В файле - " + chain.Alphabet[i];

                            return new Dictionary<string, object> { { "message", message } };
                        }
                    }

                    if (chain.GetLength() != dataBaseChain.GetLength())
                    {
                        message = "Длина цепочки в базе " + dataBaseChain.GetLength() + ", а длина цепочки из файла " +
                                  chain.GetLength();

                        return new Dictionary<string, object> { { "message", message } };
                    }

                    int[] libiadaBuilding = chain.Building;
                    int[] dataBaseBuilding = dataBaseChain.Building;

                    for (int j = 0; j < chain.GetLength(); j++)
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
