using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaWeb.Tasks;
using Newtonsoft.Json;

namespace LibiadaWeb.Controllers.Sequences
{
    public class BatchGeneticImportFromGenBankSearchFileController : AbstractResultController
    {
        public BatchGeneticImportFromGenBankSearchFileController() : base(TaskType.BatchGeneticImportFromGenBankSearchFile)
        {
        }

        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject(string.Empty);
            return View();
        }
    }
}