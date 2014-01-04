using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    public class GenesImportController : Controller
    {
        private readonly ElementRepository elementRepository;
        private readonly DnaChainRepository dnaChainRepository;
        private readonly LibiadaWebEntities db;

        public GenesImportController()
        {
            db = new LibiadaWebEntities();
            elementRepository = new ElementRepository(db);
            dnaChainRepository = new DnaChainRepository(db);
        }

        //
        // GET: /ChainCheck/

        public ActionResult Index()
        {
            ViewBag.data = new Dictionary<string, object>
                {
                    {"chains", db.dna_chain.Where(c => c.web_api_id != null).Select(c => new
                        {
                            Value = c.id,
                            Text = c.matter.name,
                            Selected = false
                        })}
                };
            return View();
        }

        [HttpPost]
        public ActionResult Index(long chainId)
        {
            dna_chain parentChain = db.dna_chain.Single(c => c.id == chainId);
            Stream stream = NcbiHelper.GetGenes(parentChain.web_api_id.ToString());
            byte[] input = new byte[stream.Length];



            // Read the file into the byte array
            stream.Read(input, 0, (int)stream.Length);

            string data = Encoding.ASCII.GetString(input);

            string[] chains = data.Split('>');

            for (int i = 0; i < chains.Length; i++)
            {
                string[] splittedFasta = chains[i].Split(new[] { '\n', '\r' });
                var chainStringBuilder = new StringBuilder();
                String fastaHeader = splittedFasta[0];
                for (int j = 1; j < splittedFasta.Length; j++)
                {
                    chainStringBuilder.Append(splittedFasta[j]);
                }

                string resultStringChain = DataTransformators.CleanFastaFile(chainStringBuilder.ToString());

                var libiadaChain = new BaseChain(resultStringChain);
                if (!elementRepository.ElementsInDb(libiadaChain.Alphabet, parentChain.notation_id))
                {
                    throw new Exception("В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                }
                /*var resultChain = new chain
                    {
                        notation_id = parentChain.notation_id,
                        creation_date = DateTime.Now,
                        matter_id = parentChain.matter_id,
                        dissimilar = false,
                        piece_type_id = !!!!!!!,
                        piece_position = !!!!!!!,
                        remote_id = !!!!!!!!,
                        remote_db_id = parentChain.remote_db_id
                    };

                long[] alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, parentChain.notation_id, false);
                dnaChainRepository.Insert(resultChain, fastaHeader, Convert.ToInt32(webApiId), alphabet, libiadaChain.Building);
            */}

            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            return View();
        }
    }
}
