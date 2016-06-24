namespace LibiadaWeb.Controllers.Calculators
{
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using Newtonsoft.Json;

    /// <summary>
    /// The subsequence comparer controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequenceComparerController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceComparerController"/> class.
        /// </summary>
        public SubsequenceComparerController() : base("Subsequence comparer")
        {
            db = new LibiadaWebEntities();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.GetSubsequencesViewData(2, int.MaxValue, "Compare"));
            return View();
        }
    }
}
