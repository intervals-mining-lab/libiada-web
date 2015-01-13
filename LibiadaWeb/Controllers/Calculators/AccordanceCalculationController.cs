using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Calculators
{
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    public class AccordanceCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

                /// <summary>
        /// Initializes a new instance of the <see cref="AlignmentController"/> class.
        /// </summary>
        public AccordanceCalculationController() : base("Alignment", "Genes alignment")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
        }

        // GET: AccordanceCalculation
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);

            var characteristicsList = db.CharacteristicType.Where(c => c.FullSequenceApplicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var links = new SelectList(db.Link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "natures", new SelectList(db.Nature, "id", "name") }, 
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "links", links }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "languages", new SelectList(db.Language, "id", "name") }, 
                    { "translators", translators }
                };
            return View();
        }
    }
}