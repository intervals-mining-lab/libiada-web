namespace LibiadaWeb.Controllers.Chains
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

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
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var matterIds = db.dna_chain.Where(c => c.product_id != null).Select(c => c.matter_id).Distinct().ToList();
            ViewBag.dbName = DbHelper.GetDbName(db);
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
            List<long> childChains =
                db.dna_chain.Where(c => c.matter_id == matterId && c.piece_type_id != Aliases.PieceTypeFullGenome)
                .OrderBy(c => c.piece_type_id).Select(c => c.id).ToList();
            var stringBuilder = new StringBuilder();
            childChains.ForEach(c => stringBuilder.Append(chainRepository.ToLibiadaChain(c)));

            if (stringBuilder.ToString() == parentChain)
            {
                TempData["check"] = true;
            }

            TempData["check"] = false;

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
            ViewBag.check = TempData["check"];
            TempData.Keep();
            return View();
        }
    }
}
