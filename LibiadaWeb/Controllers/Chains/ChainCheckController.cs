namespace LibiadaWeb.Controllers.Chains
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The chain check controller.
    /// </summary>
    public class ChainCheckController : AbstractResultController
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
        public ChainCheckController() : base("ChainCheck", "Chain check")
        {
            this.db = new LibiadaWebEntities();
            this.chainRepository = new ChainRepository(this.db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            this.ViewBag.matterId = new SelectList(this.db.matter, "id", "name");
            return this.View();
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
            var myFile = this.Request.Files[0];

            var fileLen = myFile.ContentLength;

            if (fileLen == 0)
            {
                this.ModelState.AddModelError("Error", "Файл цепочки не задан");
                return this.View();
            }

            byte[] input = new byte[fileLen];

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

            BaseChain libiadaChain = new BaseChain(resultStringChain);

            long chainId = this.db.chain.Single(c => c.matter_id == matterId).id;

            string message;

            if (!this.db.dna_chain.Any(d => d.fasta_header.Equals(fastaHeader)))
            {
                message = "объекта с заголовком " + fastaHeader + " не существует";

                this.TempData["result"] = new Dictionary<string, object> { { "message", message } };
                return this.RedirectToAction("Result");
            }

            BaseChain dataBaseChain = this.chainRepository.ToLibiadaBaseChain(chainId);

            if (dataBaseChain.Equals(libiadaChain))
            {
                message = "Цепочки в БД и в файле идентичны";
            }
            else
            {
                if (libiadaChain.Alphabet.Cardinality != dataBaseChain.Alphabet.Cardinality)
                {
                    message = "Размеры алфавитов не совпадают. В базе - " + dataBaseChain.Alphabet.Cardinality + ". В файле - " + libiadaChain.Alphabet.Cardinality;
                    
                    this.TempData["result"] = new Dictionary<string, object> { { "message", message } };
                    return this.RedirectToAction("Result");
                }

                for (int i = 0; i < libiadaChain.Alphabet.Cardinality; i++)
                {
                    if (!libiadaChain.Alphabet[i].ToString().Equals(dataBaseChain.Alphabet[i].ToString()))
                    {
                        message = i + "Элементы алфавитов не равны. В базе - " + dataBaseChain.Alphabet[i] + ". В файле - " + libiadaChain.Alphabet[i];
                        
                        this.TempData["result"] = new Dictionary<string, object> { { "message", message } };
                        return this.RedirectToAction("Result");
                    }
                }

                if (libiadaChain.GetLength() != dataBaseChain.GetLength())
                {
                    message = "Длина цепочки в базе " + dataBaseChain.GetLength() + ", а длина цепочки из файла " + libiadaChain.GetLength();

                    this.TempData["result"] = new Dictionary<string, object> { { "message", message } };
                    return this.RedirectToAction("Result");
                }

                int[] libiadaBuilding = libiadaChain.Building;
                int[] dataBaseBuilding = dataBaseChain.Building;

                for (int j = 0; j < libiadaChain.GetLength(); j++)
                {
                    if (libiadaBuilding[j] != dataBaseBuilding[j])
                    {
                        message = j + "Элементы цепочек не совпадают. В базе " + dataBaseBuilding[j] + ". В файле " + libiadaBuilding[j];
                        
                        this.TempData["result"] = new Dictionary<string, object> { { "message", message } };
                        return this.RedirectToAction("Result");
                    }
                }

                message = "Цепочки Шрёдингера - они равны и не равны одновременно";
            }

            this.TempData["result"] = new Dictionary<string, object> { { "message", message } };
            return this.RedirectToAction("Result");
        }
    }
}