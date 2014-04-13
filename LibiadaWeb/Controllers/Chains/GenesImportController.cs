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
    using LibiadaCore.Core.SimpleTypes;

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

            var products = new HashSet<string>();
            var geneTypes = new HashSet<string>();

            string stringParentChain = chainRepository.ToLBaseChain(chainId).ToString();

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
                                   ? temp2[0].Split(new[] { "complement" }, StringSplitOptions.RemoveEmptyEntries)[0]
                                   : temp2[0];
                string start = temp3.Split(new[] {"..", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)[0];
                string stop = temp3.Split(new[] {"..", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)[1];
                dnaChain.piece_position = Convert.ToInt32(start);
                dnaChain.complement = complement;
                starts.Add(Convert.ToInt32(start));
                stops.Add(Convert.ToInt32(stop));
                string currentStringChain = stringParentChain.Substring(Convert.ToInt32(start), Convert.ToInt32(stop) - Convert.ToInt32(start));
                var currentLibiadaChain = new BaseChain(currentStringChain);
                if (complement)
                {
                    Alphabet complementAlphabet = CreateComplementAlphabet(currentLibiadaChain.Alphabet);
                    currentLibiadaChain = new BaseChain(currentLibiadaChain.Building, complementAlphabet);
                }

                string sequenceType = string.Empty;
                for (int j = 1; j < temp2.Length; j++)
                {
                    if (temp2[j].Contains(start + ".." + stop))
                    {
                        sequenceType = temp2[j].Trim();
                        break;
                    }
                }

                string product = string.Empty;
                string geneType = string.Empty;
                int pieceTypeId;
                if (sequenceType.StartsWith("CDS"))
                {
                    pieceTypeId = Aliases.PieceTypeCodingSequence;
                    dnaChain.remote_id = GetValue(temp2, "/protein_id=\"");
                    dnaChain.web_api_id = Convert.ToInt32(GetValue(temp2, "/db_xref=\"GI:"));
                    dnaChain.description = GetValue(temp2, "/product=\"", "\"");
                    geneType = GetValue(temp2, "/gene=\"");
                    product = dnaChain.description;
                }
                else if (sequenceType.StartsWith("tRNA"))
                {
                    pieceTypeId = Aliases.PieceTypeTRNA;
                    dnaChain.description = GetValue(temp2, "/product=\"", "\"");
                    product = dnaChain.description;
                }
                else if (sequenceType.StartsWith("ncRNA"))
                {
                    pieceTypeId = Aliases.PieceTypeNCRNA;
                    dnaChain.description = GetValue(temp2, "/note=\"");
                    geneType = GetValue(temp2, "/gene=\"");
                    product = dnaChain.description;
                }
                else if (sequenceType.StartsWith("rRNA"))
                {
                    pieceTypeId = Aliases.PieceTypeRRNA;
                    dnaChain.description = GetValue(temp2, "/product=\"", "\"");
                    geneType = GetValue(temp2, "/gene=\"");
                    product = dnaChain.description;
                }
                else if (sequenceType.StartsWith("tmRNA"))
                {
                    pieceTypeId = Aliases.PieceTypeTMRNA;
                    geneType = GetValue(temp2, "/gene=\"");
                }
                else if (sequenceType.StartsWith("/rpt_type=tandem"))
                {
                    throw new Exception("Тип недоделан" + sequenceType);
                }
                else if (string.IsNullOrEmpty(sequenceType) && temp2.Last().Trim().Equals("/pseudo"))
                {
                    pieceTypeId = Aliases.PieceTypePseudoGen;
                    dnaChain.description = GetValue(temp2, "/note=\"");
                }
                else
                {
                    throw new Exception("Ни один из типов не найден. Тип:" + sequenceType);
                }

                var resultChain = new chain
                    {
                        notation_id = parentChain.notation_id,
                        matter_id = parentChain.matter_id,
                        dissimilar = false,
                        piece_type_id = pieceTypeId,
                        piece_position = Convert.ToInt64(start),
                        remote_db_id = parentChain.remote_db_id
                    };

                int productId;

                if (db.product.Any(p => p.name.Equals(product)))
                {
                    productId = db.product.Single(p => p.name.Equals(product)).id;
                }
                else
                {
                    var newProduct = new product { name = product };
                    db.product.AddObject(newProduct);
                    db.SaveChanges();
                    productId = newProduct.id;
                }

                long[] alphabet = elementRepository.ToDbElements(currentLibiadaChain.Alphabet, parentChain.notation_id, false);
                dnaChainRepository.Insert(resultChain, null, null, productId, complement, false, alphabet, currentLibiadaChain.Building);
            }
           
            TempData["products"] = products.ToArray();
       
            TempData["genes"] = geneTypes.ToArray();

            return RedirectToAction("Result");
        }

        private Alphabet CreateComplementAlphabet(Alphabet alphabet)
        {
            var newAlphabet = new Alphabet();
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                newAlphabet.Add(this.GetComplementElement(alphabet[i]));
            }
            return newAlphabet;
        }

        private ValueChar GetComplementElement(IBaseObject source)
        {
            switch (source.ToString())
            {
                case "A":
                case "a":
                    return new ValueChar('T');
                case "C":
                case "c":
                    return new ValueChar('G');
                case "G":
                case "g":
                    return new ValueChar('C');
                case "T":
                case "t":
                    return new ValueChar('A');
                default:
                    throw new ArgumentException("Unknown nucleotide.", "source");
            }
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
            string result = string.Empty;
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
