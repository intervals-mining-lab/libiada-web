namespace LibiadaWeb.Controllers.Chains
{
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The chain check controller.
    /// </summary>
    public class ChainCheckController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainCheckController"/> class.
        /// </summary>
        public ChainCheckController()
        {
            db = new LibiadaWebEntities();
            chainRepository = new ChainRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.matterId = new SelectList(db.matter, "id", "name");
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
            var myFile = Request.Files[0];

            var fileLen = myFile.ContentLength;

            if (fileLen == 0)
            {
                ModelState.AddModelError("Error", "Файл цепочки не задан");
                return View();
            }

            byte[] input = new byte[fileLen];

            // Initialize the stream.
            var fileStream = myFile.InputStream;

            // Read the file into the byte array.
            fileStream.Read(input, 0, fileLen);

            // Copy the byte array into a string.
            string stringChain = Encoding.ASCII.GetString(input);
            string[] tempString = stringChain.Split(new[] { '\n', '\r' });

            var chainStringBuilder = new StringBuilder();
            string fastaHeader = tempString[0];
            for (int j = 1; j < tempString.Length; j++)
            {
                chainStringBuilder.Append(tempString[j]);
            }

            string resultStringChain = DataTransformers.CleanFastaFile(chainStringBuilder.ToString());

            BaseChain libiadaChain = new BaseChain(resultStringChain);

            long chainId = db.chain.Single(c => c.matter_id == matterId).id;

            if (!db.dna_chain.Any(d => d.fasta_header.Equals(fastaHeader)))
            {
                TempData["message"] = "объекта с заголовком " + fastaHeader + " не существует";
                return RedirectToAction("Result");
            }
            BaseChain dataBaseChain = chainRepository.ToLBaseChain(chainId);
            if (dataBaseChain.Equals(libiadaChain))
            {
                TempData["message"] = "Цепочки в БД и в файле идентичны";
            }
            else
            {
                if (libiadaChain.Alphabet.Cardinality != dataBaseChain.Alphabet.Cardinality)
                {
                    TempData["message"] = " Размеры алфавитов не совпадают. В базе - " + dataBaseChain.Alphabet.Cardinality
                                          + ". В файле - " + libiadaChain.Alphabet.Cardinality;
                    return RedirectToAction("Result");
                }
                for (int i = 0; i < libiadaChain.Alphabet.Cardinality; i++)
                {
                    if (!libiadaChain.Alphabet[i].ToString().Equals(dataBaseChain.Alphabet[i].ToString()))
                    {
                        TempData["message"] = i + " элементы алфавитов не равны. В базе - " +
                                              dataBaseChain.Alphabet[i] + ". В файле -" + libiadaChain.Alphabet[i];
                        return RedirectToAction("Result");
                    }
                }
                if (libiadaChain.GetLength() != dataBaseChain.GetLength())
                {
                    TempData["message"] = "Длина цепочки в базе " + dataBaseChain.GetLength() +
                                          ", а длина цепочки из файла " + libiadaChain.GetLength();
                }
                int[] libiadaBuilding = libiadaChain.Building;
                int[] dataBaseBuilding = dataBaseChain.Building;
                for (int j = 0; j < libiadaChain.GetLength(); j++)
                {
                    if (libiadaBuilding[j] != dataBaseBuilding[j])
                    {
                        TempData["message"] = j + " элементы цепочек не совпадают. В базе " +
                                              dataBaseBuilding[j] + ". В файле " + libiadaBuilding[j];
                        return RedirectToAction("Result");
                    }
                    TempData["message"] = "Цепочки Шрёдингера - они равны и неравны одновременно";
                }
            }

            return RedirectToAction("Result");
        }

        /// <summary>
        /// The result.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Result()
        {
            ViewBag.message = TempData["message"] as string;
            TempData.Keep();
            return View();
        }
    }
}