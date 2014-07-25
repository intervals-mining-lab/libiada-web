namespace LibiadaWeb.Controllers.Chains
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The genes import controller.
    /// </summary>
    public class GenesImportController : Controller
    {
        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// The dna chain repository.
        /// </summary>
        private readonly DnaChainRepository dnaChainRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesImportController"/> class.
        /// </summary>
        public GenesImportController()
        {
            db = new LibiadaWebEntities();
            elementRepository = new ElementRepository(db);
            dnaChainRepository = new DnaChainRepository(db);
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
            ViewBag.dbName = DbHelper.GetDbName(db);
            ViewBag.data = new Dictionary<string, object>
                {
                    {
                        "chains", db.dna_chain.Where(c => c.web_api_id != null).Select(c => new
                        {
                            Value = c.id,
                            Text = c.matter.name,
                            Selected = false
                        }).OrderBy(c => c.Text)
                    }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="chainId">
        /// The chain id.
        /// </param>
        /// <param name="localFile">
        /// The local file.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
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

            var input = new byte[stream.Length];

            // Read the file into the byte array
            stream.Read(input, 0, (int)stream.Length);

            string data = Encoding.ASCII.GetString(input);

            data = data.Split(new[] { "ORIGIN" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] temp = data.Split(new[] { "FEATURES" }, StringSplitOptions.RemoveEmptyEntries);
            string[] genes = temp[1].Split(
                new[] { "gene            ", "repeat_region   " },
                StringSplitOptions.RemoveEmptyEntries);
            var starts = new List<int>();
            var stops = new List<int> { 0 };

            string stringParentChain = chainRepository.ToLBaseChain(chainId).ToString();

            var existingChainsPositions = db.dna_chain.Where(c => c.matter_id == parentChain.matter_id 
                                                            && c.notation_id == parentChain.notation_id 
                                                            && c.piece_type_id != Aliases.PieceTypeFullGenome)
                .Select(c => c.piece_position).ToArray();
            var products = db.product.ToList();

            for (int i = 1; i < genes.Length; i++)
            {
                string[] temp2 = genes[i].Trim()
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                bool complement = temp2[0].StartsWith("complement");
                string temp3 = complement
                                   ? temp2[0].Split(
                                       new[] { "complement" },
                                       StringSplitOptions.RemoveEmptyEntries)[0]
                                   : temp2[0];
                string stringStart =
                    temp3.Split(new[] { "..", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[0];
                string stringStop =
                    temp3.Split(new[] { "..", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
                starts.Add(Convert.ToInt32(stringStart) - 1);
                stops.Add(Convert.ToInt32(stringStop) - 1);

                int start = starts.Last();

                if (!existingChainsPositions.Contains(start))
                {
                    string sequenceType = string.Empty;
                    for (int j = 1; j < temp2.Length; j++)
                    {
                        if (temp2[j].Contains(stringStart + ".." + stringStop))
                        {
                            sequenceType = temp2[j].Trim();
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(sequenceType))
                    {
                        sequenceType = temp2[temp2.Length - 1].Trim();
                    }

                    int pieceTypeId;
                    string product = string.Empty;
                    string geneType = string.Empty;
                    string description = string.Empty;

                    if (sequenceType.StartsWith("CDS"))
                    {
                        pieceTypeId = Aliases.PieceTypeCodingSequence;
                        product = GetValue(temp2, "/product=\"", "\"");
                        geneType = GetValue(temp2, "/gene=\"");
                        description = geneType;
                    }
                    else if (sequenceType.StartsWith("tRNA"))
                    {
                        pieceTypeId = Aliases.PieceTypeTRNA;
                        product = GetValue(temp2, "/product=\"", "\"");
                        description = product;
                    }
                    else if (sequenceType.StartsWith("ncRNA"))
                    {
                        pieceTypeId = Aliases.PieceTypeNCRNA;
                        description = GetValue(temp2, "/note=\"");
                        geneType = GetValue(temp2, "/gene=\"");
                    }
                    else if (sequenceType.StartsWith("rRNA"))
                    {
                        pieceTypeId = Aliases.PieceTypeRRNA;
                        product = GetValue(temp2, "/product=\"", "\"");
                        geneType = GetValue(temp2, "/gene=\"");
                        description = geneType;
                    }
                    else if (sequenceType.StartsWith("tmRNA"))
                    {
                        pieceTypeId = Aliases.PieceTypeTMRNA;
                        geneType = GetValue(temp2, "/gene=\"");
                        description = geneType;
                    }
                    else if (sequenceType.StartsWith("/rpt_type=tandem"))
                    {
                        pieceTypeId = Aliases.PieceTypeRepeatRegion;
                        description = GetValue(temp2, "/inference=\"", "\"");
                    }
                    else if (sequenceType.StartsWith("/rpt_family="))
                    {
                        pieceTypeId = Aliases.PieceTypeRepeatRegion;
                        description = GetValue(temp2, "/rpt_family=\"", "\"");
                    }
                    else if (sequenceType.StartsWith("/note=\"REP"))
                    {
                        pieceTypeId = Aliases.PieceTypeRepeatRegion;
                        description = GetValue(temp2, "/note=\"", "\"");
                    }
                    else if (sequenceType.StartsWith("/pseudo")
                             || (string.IsNullOrEmpty(sequenceType) && temp2.Last().Trim().Equals("/pseudo")))
                    {
                        pieceTypeId = Aliases.PieceTypePseudoGen;
                        description = GetValue(temp2, "/note=\"");
                    }
                    else
                    {
                        throw new Exception("Ни один из типов не найден. Тип:" + sequenceType);
                    }

                    string currentStringChain = stringParentChain.Substring(
                        starts.Last(),
                        stops.Last() - starts.Last());
                    var currentLibiadaChain = new BaseChain(currentStringChain);

                    if (complement)
                    {
                        Alphabet complementAlphabet = dnaChainRepository.CreateComplementAlphabet(currentLibiadaChain.Alphabet);
                        currentLibiadaChain = new BaseChain(currentLibiadaChain.Building, complementAlphabet);
                    }

                    var resultChain = new chain
                                          {
                                              notation_id = parentChain.notation_id,
                                              matter_id = parentChain.matter_id,
                                              description = description,
                                              dissimilar = false,
                                              piece_type_id = pieceTypeId,
                                              piece_position = starts.Last()
                                          };

                    int productId;

                    if (products.Any(p => p.name.Equals(product)))
                    {
                        productId = products.Single(p => p.name.Equals(product)).id;
                    }
                    else
                    {
                        var newProduct = new product { name = product, piece_type_id = pieceTypeId };
                        db.product.Add(newProduct);
                        db.SaveChanges();
                        products.Add(newProduct);
                        productId = newProduct.id;
                    }

                    long[] alphabet = elementRepository.ToDbElements(
                        currentLibiadaChain.Alphabet,
                        parentChain.notation_id,
                        false);
                    dnaChainRepository.Insert(
                        resultChain,
                        null,
                        null,
                        productId,
                        complement,
                        false,
                        alphabet,
                        currentLibiadaChain.Building);

                }
            }

            starts.Add(stringParentChain.Length);

            for (int j = 0; j < stops.Count; j++)
            {
                int stop = stops[j];
                if (starts[j] > stops[j] && !existingChainsPositions.Contains(stop))
                {
                    string currentStringChain = stringParentChain.Substring(stops[j], starts[j] - stops[j]);
                    var currentLibiadaChain = new BaseChain(currentStringChain);
                    var resultChain = new chain
                                          {
                                              notation_id = parentChain.notation_id,
                                              matter_id = parentChain.matter_id,
                                              description = "Non coding sequence from " + stops[j],
                                              dissimilar = false,
                                              piece_type_id = Aliases.PieceTypeNonCodingSequence,
                                              piece_position = stops[j]
                                          };

                    long[] alphabet = elementRepository.ToDbElements(
                        currentLibiadaChain.Alphabet,
                        parentChain.notation_id,
                        false);

                    dnaChainRepository.Insert(
                        resultChain,
                        null,
                        null,
                        null,
                        false,
                        false,
                        alphabet,
                        currentLibiadaChain.Building);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        

        

        /// <summary>
        /// The get value.
        /// </summary>
        /// <param name="strings">
        /// The strings.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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

        /// <summary>
        /// The get value.
        /// </summary>
        /// <param name="strings">
        /// The strings.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="endPattern">
        /// The end pattern.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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
    }
}
