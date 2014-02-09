using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
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

            data = data.Split(new[] { "ORIGIN" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] temp = data.Split(new[] { "FEATURES" }, StringSplitOptions.RemoveEmptyEntries);
            string information = temp[0];
            string[] genes = temp[1].Split(new[] { "gene            ", "repeat_region   " }, StringSplitOptions.RemoveEmptyEntries);
            var coordinates = new List<int[]>();
            for (int i = 1; i < genes.Length; i++)
            {
                var dnaChain = new dna_chain
                {
                    matter_id = parentChain.matter_id,
                    notation_id = Aliases.NotationNucleotide,
                    dissimilar = false,
                    remote_db_id = Aliases.RemoteDbNcbi,
                    partial = false
                };

                String[] temp2 = genes[i].Split(new[] { '\n', '\r' });
                bool complement = temp2[0].StartsWith("complement");
                string temp3 = complement
                                   ? temp2[0].Split(new[] {"complement"}, StringSplitOptions.RemoveEmptyEntries)[0]
                                   : temp2[0];
                string start = temp3.Split(new[] {"..", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)[0];
                String stop = temp3.Split(new[] { "..", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
                dnaChain.piece_position = Convert.ToInt32(start);
                dnaChain.complement = complement;
                coordinates.Add(new[]
                    {
                        Convert.ToInt32(start), 
                        Convert.ToInt32(stop)
                    });
                var sequenceType = temp2[3];
                if (sequenceType.StartsWith("CDS"))
                {
                    var proteinId = genes[i].Substring(genes[i].IndexOf("/protein_id=\"") + "/protein_id=\"".Length,genes[i].IndexOf("/db_xref=\""));
                    var dbXref = genes[i].Substring(genes[i].IndexOf("/db_xref=\"GI:") + "/db_xref=\"GI:".Length, genes[i].IndexOf("/translation=\""));
                    dnaChain.remote_id = proteinId;
                    dnaChain.web_api_id = Convert.ToInt32(dbXref);

                }
                else if (sequenceType.StartsWith("tRNA"))
                {
                    var product = genes[i].Substring(genes[i].IndexOf("/product=\"") + "/product=\"".Length,genes[i].IndexOf("/inference=\""));
                    dnaChain.description = product;
                }
                else if (sequenceType.StartsWith("rRNA"))
                {
                    var product = genes[i].Substring(genes[i].IndexOf("/product=\"") + "/product=\"".Length,genes[i].IndexOf("/inference=\""));
                    dnaChain.description = product;
                }
                else if (sequenceType.StartsWith("/rpt_type=tandem"))
                {

                }

            }

            /*
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

                string resultStringChain = DataTransformers.CleanFastaFile(chainStringBuilder.ToString());

                var libiadaChain = new BaseChain(resultStringChain);
                if (!elementRepository.ElementsInDb(libiadaChain.Alphabet, parentChain.notation_id))
                {
                    throw new Exception("В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                }
                var resultChain = new chain
                    {
                        notation_id = parentChain.notation_id,
                        created = DateTime.Now,
                        matter_id = parentChain.matter_id,
                        dissimilar = false,
                        piece_type_id = !!!!!!!,
                        piece_position = !!!!!!!,
                        remote_id = !!!!!!!!,
                        remote_db_id = parentChain.remote_db_id
                    };

                long[] alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, parentChain.notation_id, false);
                dnaChainRepository.Insert(resultChain, fastaHeader, Convert.ToInt32(webApiId), alphabet, libiadaChain.Building);
            
        }*/

            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            return View();
        }
    }
}
