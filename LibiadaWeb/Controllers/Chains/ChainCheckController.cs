using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;

namespace LibiadaWeb.Controllers.Chains
{
    public class ChainCheckController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /ChainCheck/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string[] file)
        {
            var myFile = Request.Files[0];

            var fileLen = myFile.ContentLength;
            byte[] input = new byte[fileLen];

            // Initialize the stream.
            var fileStream = myFile.InputStream;

            // Read the file into the byte array.
            fileStream.Read(input, 0, fileLen);

            // Copy the byte array into a string.
            string stringChain = Encoding.ASCII.GetString(input);
            var tempString = stringChain.Split('\n');
            String fastaHeader = tempString[0];
            stringChain = tempString[tempString.Length - 1];
            BaseChain libiadaChain = new BaseChain(stringChain);

            if (!db.matter.Any(m => m.description == fastaHeader))
            {
                TempData["message"] = "объекта с заголовком " + fastaHeader + " не существует";
                return RedirectToAction("Result");
            }
            Int64 matterId = db.matter.Single(m => m.description == fastaHeader).id;
            chain dbChain = db.chain.Single(c => c.matter_id == matterId);
            for (int i = 0; i < libiadaChain.Alphabet.Power; i++)
            {
                String libiadaElement = libiadaChain.Alphabet[i].ToString();
                if(!db.element.Any(e => e.value == libiadaElement && e.notation_id == 1))
                {
                    TempData["message"] = " В БД нет элемента: " + libiadaElement;
                    return RedirectToAction("Result");
                }
                element dbElement = db.element.Single(e => e.value == libiadaElement && e.notation_id == 1);
                if (!db.alphabet.Any(a => a.chain_id == dbChain.id && a.element_id == dbElement.id))
                {
                    TempData["message"] = " В БД не найден элемент алфавита: " + libiadaElement;
                    return RedirectToAction("Result");
                }
            }
            int dbChainLength = db.building.Count(b => b.chain_id == dbChain.id);
            if (libiadaChain.Building.Length != dbChainLength)
            {
                TempData["message"] = "Длина цепочки в базе " + dbChainLength + ", а длина цепочки из файла " + libiadaChain.Building.Length;
                return RedirectToAction("Result");
            }
            /*for (int j = 0; j < libiadaChain.Building.Length; j++)
            {
                if (!db.building.Any(b => b.chain_id == dbChain.id && b.index == j))
                {
                    TempData["message"] = " В БД не найден " + j + " элемент строя";
                    return RedirectToAction("Result");
                }
                int dbElementNumber = db.building.Single(b => b.chain_id == dbChain.id && b.index == j).number;
                if (dbElementNumber != libiadaChain.Building[j])
                {
                    TempData["message"] = " В БД на " + j + " позиции находится " + dbElementNumber +
                                          ", а в цепочке из файла " + libiadaChain[j];
                    return RedirectToAction("Result");
                }
            }*/
            TempData["message"] = "Цепочки в БД и в файле идентичны";
            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            ViewBag.message = TempData["message"] as String;
            return View();
        }
    }
}
