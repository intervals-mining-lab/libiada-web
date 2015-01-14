namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Xml;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

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

        private readonly MusicSequenceRepository musicSequenceRepository;

        /// <summary>
        /// The data sequence repository.
        /// </summary>
        private readonly DataSequenceRepository dataSequenceRepository;

        /// <summary>
        /// The piece type repository.
        /// </summary>
        private readonly PieceTypeRepository pieceTypeRepository;

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
            pieceTypeRepository = new PieceTypeRepository(Db);
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
            ViewBag.dbName = DbHelper.GetDbName(Db);

            var translators = new SelectList(Db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature() }, 
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
        /// <param name="productId">
        /// The product id.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <param name="complementary">
        /// The complementary.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if nature id of notation of sequence is unknown. 
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "Id,NotationId,PieceTypeId,PiecePosition,RemoteDbId,RemoteId,Description,Matter")] CommonSequence commonSequence,
            bool localFile,
            int? languageId,
            bool? original,
            int? translatorId,
            int? productId,
            bool? partial,
            bool? complementary)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int natureId = Db.Notation.Single(m => m.Id == commonSequence.NotationId).NatureId;
                    int? webApiId = null;

                    string stringSequence;

                    if (localFile)
                    {
                        var encoding = natureId == Aliases.Nature.Genetic ? Encoding.ASCII : Encoding.UTF8;
                        stringSequence = FileHelper.ReadFileStream(Request.Files[0], encoding);
                    }
                    else
                    {
                        webApiId = NcbiHelper.GetId(commonSequence.RemoteId);
                        stringSequence = NcbiHelper.GetSequenceString(webApiId.ToString());
                    }

                    switch (natureId)
                    {
                        case Aliases.Nature.Genetic:
                            dnaSequenceRepository.Create(commonSequence, productId, partial ?? false, complementary ?? false, stringSequence, webApiId);
                            break;
                        case Aliases.Nature.Music:
                            musicSequenceRepository.Create(commonSequence, stringSequence);
                            break;
                        case Aliases.Nature.Literature:
                            literatureSequenceRepository.Create(commonSequence, languageId ?? 0, original ?? false, translatorId, stringSequence);
                            break;
                        case Aliases.Nature.Data:
                            dataSequenceRepository.Create(commonSequence, stringSequence);
                            break;
                        default:
                            throw new Exception("Unknown nature.");
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("Error", e.Message);
                }
            }

            var translators = new SelectList(Db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
            {
                    { "matters", matterRepository.GetMatterSelectList(commonSequence.MatterId) }, 
                    { "notations", notationRepository.GetSelectListWithNature(commonSequence.NotationId) }, 
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature(commonSequence.PieceTypeId) }, 
                    { "languages", new SelectList(Db.Language, "id", "name", languageId) }, 
                    { "remoteDbs", commonSequence.RemoteDbId.HasValue ? 
                        remoteDbRepository.GetSelectListWithNature(commonSequence.RemoteDbId.Value) :
                        remoteDbRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(Db.Nature, "id", "name", Db.Notation.Single(m => m.Id == commonSequence.NotationId).NatureId) }, 
                    { "translators", translators }, 
                    { "natureLiterature", Aliases.Nature.Literature }
            };
            return View(commonSequence);
        }
    }
}
