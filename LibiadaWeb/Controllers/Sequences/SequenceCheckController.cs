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
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The sequence check controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequenceCheckController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceCheckController"/> class.
        /// </summary>
        public SequenceCheckController() : base(TaskType.SequenceCheck)
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            using (var db = new LibiadaWebEntities())
            {
                ViewBag.matterId = new SelectList(Cache.GetInstance().Matters.ToArray(), "id", "name");
                return View();
            }
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
        [ValidateAntiForgeryToken]
        public ActionResult Index(long matterId, string[] file)
        {
            return CreateTask(() =>
            {
                var myFile = Request.Files[0];

                if (myFile == null || myFile.ContentLength == 0)
                {
                    throw new ArgumentNullException(nameof(file), "Sequence file not found or empty.");
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

                for (int j = 1; j < tempString.Length; j++)
                {
                    sequenceStringBuilder.Append(tempString[j]);
                }

                string resultStringSequence = DataTransformers.CleanFastaFile(sequenceStringBuilder.ToString());

                var chain = new BaseChain(resultStringSequence);
                string message;
                string status;
                BaseChain dbChain;
                using (var db = new LibiadaWebEntities())
                {
                    long sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                    var commonSequenceRepository = new CommonSequenceRepository(db);
                    dbChain = commonSequenceRepository.GetLibiadaBaseChain(sequenceId);
                }

                if (dbChain.Equals(chain))
                {
                    message = "Sequence in db and in file are equal";
                    status = "Success";
                }
                else
                {
                    status = "Error";
                    if (chain.Alphabet.Cardinality != dbChain.Alphabet.Cardinality)
                    {
                        message = $"Alphabet sizes are not equal. In db - {dbChain.Alphabet.Cardinality}. In file - {chain.Alphabet.Cardinality}";
                        return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message }) } };
                    }

                    for (int i = 0; i < chain.Alphabet.Cardinality; i++)
                    {
                        if (!chain.Alphabet[i].ToString().Equals(dbChain.Alphabet[i].ToString()))
                        {
                            message = $"{i} elements in alphabet are not equal. In db - {dbChain.Alphabet[i]}. In file - {chain.Alphabet[i]}";
                            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message }) } };
                        }
                    }

                    if (chain.Length != dbChain.Length)
                    {
                        message = $"Sequence length in db {dbChain.Length}, and sequence length from file{chain.Length}";
                        return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
                    }

                    int[] libiadaBuilding = chain.Building;
                    int[] dataBaseBuilding = dbChain.Building;

                    for (int j = 0; j < chain.Length; j++)
                    {
                        if (libiadaBuilding[j] != dataBaseBuilding[j])
                        {
                            message = $"{j} sequences elements are not equal. In db {dataBaseBuilding[j]}. In file {libiadaBuilding[j]}";
                            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
                        }
                    }

                    message = "Sequences are equal and not equal at the same time.";
                }

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
            });
        }
    }
}
