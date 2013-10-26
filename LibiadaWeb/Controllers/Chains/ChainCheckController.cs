using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    public class ChainCheckController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly ChainRepository chainRepository;

        public ChainCheckController()
        {
            db = new LibiadaWebEntities();
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /ChainCheck/

        public ActionResult Index()
        {
            ViewBag.matterId = new SelectList(db.matter, "id", "name");
            return View();
        }

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

            StringBuilder chainStringBuilder = new StringBuilder();
            String fastaHeader = tempString[0];
            for (int j = 1; j < tempString.Length; j++)
            {
                chainStringBuilder.Append(tempString[j]);
            }

            string resultStringChain = DataTransformators.CleanFastaFile(chainStringBuilder.ToString());

            BaseChain libiadaChain = new BaseChain(resultStringChain);

            long chainId = db.chain.Single(c => c.matter_id == matterId).id;

            if (!db.dna_chain.Any(d => d.fasta_header.Equals(fastaHeader)))
            {
                TempData["message"] = "объекта с заголовком " + fastaHeader + " не существует";
                return RedirectToAction("Result");
            }
            BaseChain dbChain = chainRepository.ToLBaseChain(chainId);
            if (dbChain.Equals(libiadaChain))
            {
                TempData["message"] = "Цепочки в БД и в файле идентичны";
            }
            else
            {
                if (libiadaChain.Alphabet.Power != dbChain.Alphabet.Power)
                {
                    TempData["message"] = " Размеры алфавитов не совпадают. В базе - " + dbChain.Alphabet.Power 
                                          + ". В файле - " + libiadaChain.Alphabet.Power;
                    return RedirectToAction("Result");
                }
                for (int i = 0; i < libiadaChain.Alphabet.Power; i++)
                {
                    if (!libiadaChain.Alphabet[i].ToString().Equals(dbChain.Alphabet[i].ToString()))
                    {
                        TempData["message"] = i + " элементы алфавитов не равны. В базе - " + 
                                              dbChain.Alphabet[i] + ". В файле -" + libiadaChain.Alphabet[i];
                        return RedirectToAction("Result");
                    }
                }
                
                
                if (libiadaChain.Length != dbChain.Length)
                {
                    TempData["message"] = "Длина цепочки в базе " + dbChain.Length + 
                                          ", а длина цепочки из файла " + libiadaChain.Length;
                }
                int[] libiadaBuilding = libiadaChain.Building;
                int[] dbBuilding = dbChain.Building;
                for (int j = 0; j < libiadaChain.Length; j++)
                {
                    if (libiadaBuilding[j] != dbBuilding[j])
                    {
                        TempData["message"] = j + " элементы цепочек не совпадают. В базе " + 
                                              dbBuilding[j] + ". В файле " + libiadaBuilding[j];
                        return RedirectToAction("Result");
                    }
                    TempData["message"] = "Цепочки Шрёдингера - они равны и неравны одновременно";
                }
            }
            
            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            ViewBag.message = TempData["message"] as String;
            TempData.Keep();
            return View();
        }
    }
}
