
using System.Collections.Generic;
using System.Web.Mvc;
using LibiadaWeb.Extensions;
using LibiadaWeb.Tasks;
using Newtonsoft.Json;

namespace LibiadaWeb.Controllers.Sequences
{
    public class BatchPoemsImportController : AbstractResultController
    {
        public BatchPoemsImportController() : base(TaskType.BatchPoemsImport)
        {
        }

        // GET: BatchPoemsImport
        public ActionResult Index()
        {
            var viewData = new Dictionary<string, object>
            {
                { "notations", new [] { Notation.Letters, Notation.Consonance }.ToSelectListWithNature() }
            };
            ViewBag.data = JsonConvert.SerializeObject(viewData);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Notation notation)
        {
            return View();
        }
    }
}
