namespace LibiadaWeb.Controllers.Chains
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The genes import controller.
    /// </summary>
    public class GenesImportCheckController : Controller
    {
        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly DnaChainRepository dnaChainRepository;

        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesImportCheckController"/> class.
        /// </summary>
        public GenesImportCheckController()
        {
            db = new LibiadaWebEntities();
            chainRepository = new ChainRepository(db);
            dnaChainRepository = new DnaChainRepository(db);
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

            var matterIds = db.dna_chain.Where(c => c.product_id != null).Select(c => c.matter_id).Distinct().ToList();
            ViewBag.data = new Dictionary<string, object>
                {
                    {
                        "chains", db.dna_chain.Where(c => c.piece_type_id == Aliases.PieceTypeFullGenome && matterIds.Contains(c.matter_id))
                        .Select(c => new
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
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        [HttpPost]
        public ActionResult Index(long chainId)
        {
            long matterId = db.dna_chain.Single(c => c.id == chainId).matter_id;
            string parentChain = chainRepository.ToLibiadaChain(chainId).ToString();
            var childChains =
                db.dna_chain.Where(c => c.matter_id == matterId && c.piece_type_id != Aliases.PieceTypeFullGenome)
                .OrderBy(c => c.piece_position).ToList();

            var stringBuilder = new StringBuilder();
            var intersections = new List<long[]>();

            for (int i = 0; i < childChains.Count - 1; i++)
            {
                var tempChain = chainRepository.ToLibiadaChain(childChains[i].id);

                if (childChains[i].complement)
                {
                    tempChain = new Chain(tempChain.Building, dnaChainRepository.CreateComplementAlphabet(tempChain.Alphabet));
                }

                var chain = tempChain.ToString();
                var intersection = childChains[i].piece_position + chain.Length - childChains[i + 1].piece_position;

                if (intersection > 0)
                {
                    chain = chain.Substring(0, (int)(chain.Length - intersection));
                }

                stringBuilder.Append(chain);

                intersections.Add(new[] { childChains[i].piece_position, stringBuilder.Length, childChains[i].piece_position + chain.Length, intersection});
            }

            var tempChain2 = chainRepository.ToLibiadaChain(childChains[childChains.Count - 1].id);

            if (childChains[childChains.Count - 1].complement)
            {
                tempChain2 = new Chain(tempChain2.Building, dnaChainRepository.CreateComplementAlphabet(tempChain2.Alphabet));
            }

            stringBuilder.Append(tempChain2.ToString());
            string gluedChain = stringBuilder.ToString();

            TempData["intersections"] = intersections;

            if (string.Equals(gluedChain, parentChain))
            {
                TempData["check"] = true;
            }
            else
            {
                TempData["check"] = false;
                if (gluedChain.Length < parentChain.Length)
                {
                    TempData["lengthDelta"] = parentChain.Length - gluedChain.Length;
                }
                else
                {
                    TempData["lengthDelta"] = 0;
                    var notEqualPositions = new List<int>();
                    for (int j = 0; j < gluedChain.Length; j++)
                    {
                        if (!gluedChain[j].Equals(parentChain[j]))
                        {
                            notEqualPositions.Add(j);
                        }
                    }

                    TempData["NotEqualPositions"] = notEqualPositions;
                }
            }

            return RedirectToAction("Result", "GenesImportCheck");
        }

        /// <summary>
        /// The result.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public ActionResult Result()
        {
            ViewBag.intersections = TempData["intersections"];
            ViewBag.check = TempData["check"];
            ViewBag.lengthDelta = TempData["lengthDelta"];
            ViewBag.NotEqualPositions = TempData["NotEqualPositions"];
            TempData.Keep();
            return View();
        }
    }
}
