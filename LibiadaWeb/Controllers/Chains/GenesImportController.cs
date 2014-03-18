using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    using LibiadaCore.Core;

    public class GenesImportController : Controller
    {
        private readonly ElementRepository elementRepository;
        private readonly DnaChainRepository dnaChainRepository;
        private readonly ChainRepository chainRepository;
        private readonly LibiadaWebEntities db;

        public GenesImportController()
        {
            db = new LibiadaWebEntities();
            elementRepository = new ElementRepository(db);
            dnaChainRepository = new DnaChainRepository(db);
            chainRepository = new ChainRepository(db);
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
        public ActionResult Index(long chainId, bool localFile)
        { 
            
            dna_chain parentChain = db.dna_chain.Single(c => c.id == chainId);
            Stream stream;
            if (localFile)
            {
                HttpPostedFileBase file = Request.Files[0];

                if (file == null || file.ContentLength == 0)
                {
                    throw new ArgumentNullException("Файл цепочки не задан или пуст");
                }
                stream = file.InputStream;
            }
            else
            {
                stream = NcbiHelper.GetGenes(parentChain.web_api_id.ToString());
            }
            byte[] input = new byte[stream.Length];

            // Read the file into the byte array
            stream.Read(input, 0, (int)stream.Length);

            string data = Encoding.ASCII.GetString(input);

            data = data.Split(new[] { "ORIGIN" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] temp = data.Split(new[] { "FEATURES" }, StringSplitOptions.RemoveEmptyEntries);
            string information = temp[0];
            string[] genes = temp[1].Split(new[] { "gene            ", "repeat_region   " }, StringSplitOptions.RemoveEmptyEntries);
            var starts = new List<int>();
            var stops = new List<int>();

            HashSet<string> products = new HashSet<string>();
            HashSet<string> geneTypes = new HashSet<string>();

            BaseChain dbChain = chainRepository.ToLBaseChain(chainId);
            string dbStringChain = dbChain.ToString();

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

                string[] temp2 = genes[i].Trim().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                bool complement = temp2[0].StartsWith("complement");
                string temp3 = complement
                                   ? temp2[0].Split(new[] {"complement"}, StringSplitOptions.RemoveEmptyEntries)[0]
                                   : temp2[0];
                string start = temp3.Split(new[] {"..", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)[0];
                string stop = temp3.Split(new[] {"..", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)[1];
                dnaChain.piece_position = Convert.ToInt32(start);
                dnaChain.complement = complement;
                starts.Add(Convert.ToInt32(start));
                stops.Add(Convert.ToInt32(stop));
                string sequenceType = string.Empty;
                for (int j = 1; j < temp2.Length; j++)
                {
                    if (temp2[j].Contains(start + ".." + stop))
                    {
                        sequenceType = temp2[j].Trim();
                        break;
                    }
                }
                if (sequenceType.StartsWith("CDS"))
                {
                    dnaChain.remote_id = GetValue(temp2, "/protein_id=\"");
                    dnaChain.web_api_id = Convert.ToInt32(GetValue(temp2, "/db_xref=\"GI:"));
                    dnaChain.description = GetValue(temp2, "/product=\"", "\"");
                    geneTypes.Add(GetValue(temp2, "/gene=\""));
                    products.Add(dnaChain.description);
                }
                else if (sequenceType.StartsWith("tRNA"))
                {
                    dnaChain.description = GetValue(temp2, "/product=\"", "\"");
                    products.Add(dnaChain.description);
                }
                else if (sequenceType.StartsWith("ncRNA"))
                {
                    dnaChain.description = GetValue(temp2, "/note=\"");
                    geneTypes.Add(GetValue(temp2, "/gene=\""));
                    //products.Add(dnaChain.description);
                }
                else if (sequenceType.StartsWith("rRNA"))
                {
                    dnaChain.description = GetValue(temp2, "/product=\"", "\"");
                    geneTypes.Add(GetValue(temp2, "/gene=\""));
                    products.Add(dnaChain.description);
                }
                else if (sequenceType.StartsWith("tmRNA"))
                {
                    geneTypes.Add(GetValue(temp2, "/gene=\""));
                }
                else if (sequenceType.StartsWith("/rpt_type=tandem"))
                {

                }
                else if (string.IsNullOrEmpty(sequenceType) && temp2.Last().Trim().Equals("/pseudo"))
                {
                    dnaChain.description = GetValue(temp2, "/note=\"");
                }
                else
                {
                    throw new Exception("Ни один из типов не найден. Тип:" + sequenceType);
                }
            }
           // var subChains = DiffCutter.Cut(dbStringChain, new DefaultCutRule(starts, stops));
            TempData["products"] = products.ToArray();
            //TempData["subChains"] = subChains.ToArray();
            TempData["genes"] = geneTypes.ToArray();

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

        private static string GetValue(string[] strings, string pattern)
        {
            for (int i = 1; i < strings.Length; i++)
            {
                if (strings[i].Contains(pattern))
                {
                    return strings[i].Substring(strings[i].IndexOf(pattern) + pattern.Length, strings[i].Length - strings[i].IndexOf(pattern) - pattern.Length - 1);
                }
            }
            return string.Empty;
        }

        private static string GetValue(string[] strings, string pattern, string endPattern)
        {
            string result = String.Empty;
            for (int i = 1; i < strings.Length; i++)
            {
                if (strings[i].Contains(pattern))
                {
                    result += strings[i].Substring(
                        strings[i].IndexOf(pattern) + pattern.Length,
                        strings[i].Length - strings[i].IndexOf(pattern) - pattern.Length);
                    if (!strings[i].EndsWith(endPattern))
                    {
                        for (int j = i + 1; j < strings.Length; j++)
                        {
                            if (!strings[j].Contains(endPattern))
                            {
                                result += strings[j].Trim();
                            }
                            else
                            {
                                result += strings[j].Substring(0, strings[j].Length - 1).Trim();
                                return result;
                            }
                        }
                    }
                    else
                    {
                        return result.Substring(0, result.Length - 1);
                    }
                    
                }
            }
            return string.Empty;
        }

        public ActionResult Result()
        {
            ViewBag.Genes = TempData["genes"];
            ViewBag.Products = TempData["products"];
            ViewBag.SubChains = TempData["subChains"];
            TempData.Keep();
            return View();
        }
    }
}
