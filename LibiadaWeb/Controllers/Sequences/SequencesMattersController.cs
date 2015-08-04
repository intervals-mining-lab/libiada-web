namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The sequences matters controller.
    /// </summary>
    public abstract class SequencesMattersController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        protected readonly LibiadaWebEntities Db;

        /// <summary>
        /// The dna sequence repository.
        /// </summary>
        private readonly DnaSequenceRepository dnaSequenceRepository;

        /// <summary>
        /// The literature sequence repository.
        /// </summary>
        private readonly LiteratureSequenceRepository literatureSequenceRepository;

        /// <summary>
        /// The music sequence repository.
        /// </summary>
        private readonly MusicSequenceRepository musicSequenceRepository;

        /// <summary>
        /// The data sequence repository.
        /// </summary>
        private readonly DataSequenceRepository dataSequenceRepository;

        /// <summary>
        /// The feature repository.
        /// </summary>
        private readonly FeatureRepository featureRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The remote db repository.
        /// </summary>
        private readonly RemoteDbRepository remoteDbRepository;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesMattersController"/> class.
        /// </summary>
        protected SequencesMattersController()
        {
            Db = new LibiadaWebEntities();
            dnaSequenceRepository = new DnaSequenceRepository(Db);
            literatureSequenceRepository = new LiteratureSequenceRepository(Db);
            musicSequenceRepository = new MusicSequenceRepository(Db);
            dataSequenceRepository = new DataSequenceRepository(Db);
            featureRepository = new FeatureRepository(Db);
            notationRepository = new NotationRepository(Db);
            remoteDbRepository = new RemoteDbRepository(Db);
            matterRepository = new MatterRepository(Db);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            var translators = new SelectList(Db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "features", featureRepository.GetSelectListWithNature() }, 
                    { "languages", new SelectList(Db.Language, "id", "name") }, 
                    { "remoteDbs", remoteDbRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(Db.Nature, "id", "name") }, 
                    { "translators", translators }, 
                    { "natureLiterature", Aliases.Nature.Literature }
                };
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="commonSequence">
        /// The sequence.
        /// </param>
        /// <param name="localFile">
        /// The local file.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="translatorId">
        /// The translator id.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <param name="complementary">
        /// The complementary.
        /// </param>
        /// <param name="precision">
        /// Precision of data sequence.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "Id,NotationId,FeatureId,PiecePosition,RemoteDbId,RemoteId,Description,Matter")] CommonSequence commonSequence,
            bool localFile,
            int? languageId,
            bool? original,
            int? translatorId,
            bool? partial,
            bool? complementary,
            int? precision)
        {
            string errorMessage = string.Empty;
            if (ModelState.IsValid)
            {
                try
                {
                    int? webApiId = null;
                    Stream sequenceStream;

                    if (localFile)
                    {
                        sequenceStream = FileHelper.GetFileStream(Request.Files[0]);
                    }
                    else
                    {
                        webApiId = NcbiHelper.GetId(commonSequence.RemoteId);
                        sequenceStream = NcbiHelper.GetFileStream(webApiId.ToString());
                    }

                    switch (Db.Notation.Single(m => m.Id == commonSequence.NotationId).NatureId)
                    {
                        case Aliases.Nature.Genetic:
                            dnaSequenceRepository.Create(commonSequence, sequenceStream, partial ?? false, complementary ?? false, webApiId);
                            break;
                        case Aliases.Nature.Music:
                            musicSequenceRepository.Create(commonSequence, sequenceStream);
                            break;
                        case Aliases.Nature.Literature:
                            literatureSequenceRepository.Create(commonSequence, sequenceStream, languageId ?? 0, original ?? false, translatorId);
                            break;
                        case Aliases.Nature.Data:
                            dataSequenceRepository.Create(commonSequence, sequenceStream, precision ?? 0);
                            break;
                        default:
                            throw new Exception("Unknown nature.");
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("Error", e.Message);
                    errorMessage = e.Message;
                }
            }

            var translators = new SelectList(Db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            var remoteDbs = commonSequence.RemoteDbId.HasValue
                                ? remoteDbRepository.GetSelectListWithNature(commonSequence.RemoteDbId.Value)
                                : remoteDbRepository.GetSelectListWithNature();

            var sequenceNatureId = Db.Notation.Single(m => m.Id == commonSequence.NotationId).NatureId;

            ViewBag.data = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "matters", matterRepository.GetMatterSelectList(commonSequence.MatterId) }, 
                { "notations", notationRepository.GetSelectListWithNature(commonSequence.NotationId) }, 
                { "features", featureRepository.GetSelectListWithNature(commonSequence.FeatureId) }, 
                { "languages", new SelectList(Db.Language, "id", "name", languageId) }, 
                { "remoteDbs", remoteDbs }, 
                { "natures", new SelectList(Db.Nature, "id", "name", sequenceNatureId) }, 
                { "natureId", sequenceNatureId },
                { "translators", translators }, 
                { "natureLiterature", Aliases.Nature.Literature }
            };
            return View(commonSequence);
        }
    }
}
