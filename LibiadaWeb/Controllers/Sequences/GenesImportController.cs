namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The genes import controller.
    /// </summary>
    public class GenesImportController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesImportController"/> class.
        /// </summary>
        public GenesImportController() : base("GenesImport", "Genes Import")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            matterRepository = new MatterRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var genesSequenceIds = db.Fragment.Select(g => g.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => c.WebApiId != null && !genesSequenceIds.Contains(c.Id)).Select(c => c.MatterId);

            var matters = db.Matter.Where(m => matterIds.Contains(m.Id));

            ViewBag.data = new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", 1 },
                    { "maximumSelectedMatters", 1 },
                    { "natureId", Aliases.Nature.Genetic }, 
                    { "matters", matterRepository.GetMatterSelectList(matters) }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="localFile">
        /// The local file.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if there is no file with sequence.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if unknown part is found.
        /// </exception>
        [HttpPost]
        public ActionResult Index(long matterId, bool localFile)
        {
            return Action(() =>
            {
                long sequenceId = db.DnaSequence.Single(d => d.MatterId == matterId).Id;
                DnaSequence parentSequence = db.DnaSequence.Single(c => c.Id == sequenceId);
                Stream stream;
                if (localFile)
                {
                    HttpPostedFileBase file = Request.Files[0];

                    if (file == null || file.ContentLength == 0)
                    {
                        throw new ArgumentNullException("file", "Sequence file is empty");
                    }

                    stream = file.InputStream;
                }
                else
                {
                    stream = NcbiHelper.GetGenesFileStream(parentSequence.WebApiId.ToString());
                }

                var input = new byte[stream.Length];

                // Read the file into the byte array
                stream.Read(input, 0, (int)stream.Length);

                string data = Encoding.ASCII.GetString(input);

                data = data.Split(new[] { "ORIGIN" }, StringSplitOptions.RemoveEmptyEntries)[0];
                string[] temp = data.Split(new[] { "FEATURES" }, StringSplitOptions.RemoveEmptyEntries);
                string[] genes = temp[1].Split(new[] { "gene            ", "repeat_region  " }, StringSplitOptions.RemoveEmptyEntries);
                var starts = new List<int>();
                var stops = new List<int> { 0 };

                string stringParentChain = commonSequenceRepository.ToLibiadaBaseChain(sequenceId).ToString();

                var existingGenes = db.Fragment.Where(g => g.SequenceId == parentSequence.Id).Select(g => g.Id);

                var existingSequencesPositions = db.Position.Where(p => existingGenes.Contains(p.FragmentId)).Select(p => p.Start);
                var products = db.SequenceAttribute.ToList();

                for (int i = 1; i < genes.Length; i++)
                {
                    string[] temp2 = genes[i].Trim().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    bool complement = temp2[0].StartsWith("complement");
                    string temp3 = complement
                                       ? temp2[0].Split(new[] { "complement" }, StringSplitOptions.RemoveEmptyEntries)[0]
                                       : temp2[0];
                    string stringStart = temp3.Split(new[] { "..", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string stringStop = temp3.Split(new[] { "..", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    starts.Add(Convert.ToInt32(stringStart) - 1);
                    stops.Add(Convert.ToInt32(stringStop) - 1);

                    int start = starts.Last();

                    if (!existingSequencesPositions.Contains(start))
                    {
                        string sequenceType = string.Empty;
                        for (int j = 1; j < temp2.Length; j++)
                        {
                            if (temp2[j].Contains(stringStart + ".." + stringStop))
                            {
                                sequenceType = temp2[j].Trim();
                                break;
                            }

                            if (temp2[j].Trim().Equals("/pseudo"))
                            {
                                sequenceType = temp2[j].Trim();
                            }
                        }

                        if (genes[i][0] == ' ')
                        {
                            sequenceType = "/note=\"REP";
                        }

                        if (string.IsNullOrEmpty(sequenceType))
                        {
                            sequenceType = temp2[temp2.Length - 1].Trim();
                        }

                        int featureId;
                        string product = string.Empty;
                        string geneType = string.Empty;
                        string description = string.Empty;

                        if (sequenceType.StartsWith("CDS"))
                        {
                            featureId = Aliases.Feature.CodingSequence;
                            product = GetValue(temp2, "/product=\"", "\"");
                            geneType = GetValue(temp2, "/gene=\"");
                            description = geneType;
                        }
                        else if (sequenceType.StartsWith("tRNA"))
                        {
                            featureId = Aliases.Feature.TRNA;
                            product = GetValue(temp2, "/product=\"", "\"");
                            description = product;
                        }
                        else if (sequenceType.StartsWith("ncRNA"))
                        {
                            featureId = Aliases.Feature.NCRNA;
                            description = GetValue(temp2, "/note=\"");
                            geneType = GetValue(temp2, "/gene=\"");
                        }
                        else if (sequenceType.StartsWith("rRNA"))
                        {
                            featureId = Aliases.Feature.RRNA;
                            product = GetValue(temp2, "/product=\"", "\"");
                            geneType = GetValue(temp2, "/gene=\"");
                            description = geneType;
                        }
                        else if (sequenceType.StartsWith("tmRNA"))
                        {
                            featureId = Aliases.Feature.TMRNA;
                            geneType = GetValue(temp2, "/gene=\"");
                            description = geneType;
                        }
                        else if (sequenceType.StartsWith("/rpt_type=tandem"))
                        {
                            featureId = Aliases.Feature.RepeatRegion;
                            description = GetValue(temp2, "/inference=\"", "\"");
                        }
                        else if (sequenceType.StartsWith("/rpt_family="))
                        {
                            featureId = Aliases.Feature.RepeatRegion;
                            description = GetValue(temp2, "/rpt_family=\"", "\"");
                        }
                        else if (sequenceType.StartsWith("/note=\"REP"))
                        {
                            featureId = Aliases.Feature.RepeatRegion;
                            description = GetValue(temp2, "/note=\"", "\"");
                        }
                        else if (sequenceType.StartsWith("/pseudo")
                                 || (string.IsNullOrEmpty(sequenceType) && temp2.Last().Trim().Equals("/pseudo")))
                        {
                            featureId = Aliases.Feature.PseudoGen;
                            description = GetValue(temp2, "/note=\"");
                        }
                        else if (sequenceType.StartsWith("misc_RNA"))
                        {
                            featureId = Aliases.Feature.MiscRNA;
                            product = GetValue(temp2, "/product=\"", "\"");
                            description = GetValue(temp2, "/note=\"");
                        }
                        else
                        {
                            throw new Exception("Ни один из типов не найден. Тип:" + sequenceType);
                        }

                        SequenceAttribute dataBaseProduct;

                        //if (products.Any(p => p.Name.Equals(product)))
                        //{
                        //    dataBaseProduct = products.Single(p => p.Name.Equals(product));
                        //}
                        //else
                        //{
                        //    dataBaseProduct = new SequenceAttribute { Name = product, FeatureId = featureId };
                        //    db.SequenceAttribute.Add(dataBaseProduct);
                        //    products.Add(dataBaseProduct);
                        //}

                        var gene = new Fragment
                                       {
                                           Id = DbHelper.GetNewElementId(db),
                                           SequenceId = parentSequence.Id,
                                           FeatureId = featureId,
                                           Complementary = complement,
                                           Partial = false
                                       };

                        db.Fragment.Add(gene);

                        var piece = new Position
                                        {
                                            Fragment = gene,
                                            Start = starts.Last(),
                                            Length = stops.Last() - starts.Last()
                                        };

                        db.Position.Add(piece);
                    }
                }

                starts.Add(stringParentChain.Length);

                db.SaveChanges();

                for (int j = 0; j < stops.Count; j++)
                {
                    int stop = stops[j];
                    if (starts[j] > stops[j] && !existingSequencesPositions.Contains(stop))
                    {
                        var gene = new Fragment
                                       {
                                           Id = DbHelper.GetNewElementId(db),
                                           SequenceId = parentSequence.Id,
                                           FeatureId = Aliases.Feature.NonCodingSequence,
                                           Complementary = false,
                                           Partial = false
                                       };

                        db.Fragment.Add(gene);

                        var piece = new Position { Fragment = gene, Start = stops[j], Length = starts[j] - stops[j] };

                        db.Position.Add(piece);
                    }
                }

                db.SaveChanges();

                var matterName = db.Matter.Single(m => m.Id == matterId).Name;

                var sequenceGenes = db.Fragment.Where(g => g.SequenceId == sequenceId).Include(g => g.Position).Include(g => g.Feature).Include(g => g.SequenceAttribute);

                return new Dictionary<string, object>
                                     {
                                         { "matterName", matterName }, 
                                         { "genes", sequenceGenes }
                                     };
            });
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
        private string GetValue(string[] strings, string pattern)
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
        private string GetValue(string[] strings, string pattern, string endPattern)
        {
            string result = string.Empty;
            for (int i = 1; i < strings.Length; i++)
            {
                if (strings[i].Contains(pattern))
                {
                    result += strings[i].Substring(strings[i].IndexOf(pattern) + pattern.Length, strings[i].Length - strings[i].IndexOf(pattern) - pattern.Length);

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
