namespace LibiadaWeb.Controllers.Calculators
{
    using LibiadaWeb.Helpers;
    using Newtonsoft.Json;
    using System.Web.Mvc;

    [Authorize(Roles = "Admin")]
    public class SubsequenceComparerController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        public SubsequenceComparerController() : base("Subsequence comparer")
        {
            db = new LibiadaWebEntities();
        }

        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.GetSubsequencesViewData(2, int.MaxValue, "Compare"));
            return View();
        }
    }
}
