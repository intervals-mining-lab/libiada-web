namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Account;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The sequences matters controller.
    /// </summary>
    public abstract class SequencesMattersController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        protected readonly LibiadaWebEntities Db;

        /// <summary>
        /// The thread disposable.
        /// </summary>
        protected bool ThreadDisposable = true;

        /// <summary>
        /// The DNA sequence repository.
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
        protected SequencesMattersController() : base("Sequence upload")
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

            IEnumerable<object> notations;
            IEnumerable<object> features;
            IEnumerable<SelectListItem> natures;

            if (UserHelper.IsAdmin())
            {
                natures = EnumHelper.GetSelectList(typeof(Nature));
                notations = notationRepository.GetSelectListWithNature();
                features = featureRepository.GetSelectListWithNature();
            }
            else
            {
                natures = new List<Nature> { Nature.Genetic }.ToSelectList();
                notations = notationRepository.GetSelectListWithNature(new List<int> { Aliases.Notation.Nucleotide });
                features = featureRepository.GetSelectListWithNature(new List<int> { Aliases.Feature.FullGenome, Aliases.Feature.RibosomalRNA, Aliases.Feature.Plasmid }, new List<int>());
            }

            ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "natures", natures }, 
                    { "notations", notations }, 
                    { "features", features }, 
                    { "languages", new SelectList(Db.Language, "id", "name") }, 
                    { "remoteDbs", remoteDbRepository.GetSelectListWithNature() }, 
                    { "translators", translators }, 
                    { "natureLiterature", (byte)Nature.Literature }
                });

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
        /// <param name="precision">
        /// Precision of data sequence.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Id,NotationId,FeatureId,PiecePosition,RemoteDbId,RemoteId,Description,Matter")] CommonSequence commonSequence,
            bool localFile,
            int? languageId,
            bool? original,
            int? translatorId,
            bool? partial,
            int? precision)
        {
            ThreadDisposable = false;

            return Action(() =>
            {
                try
                {
                    string errorMessage = string.Empty;
                    if (ModelState.IsValid)
                    {
                        var sequenceStream = localFile ? 
                            FileHelper.GetFileStream(this.Request.Files[0]) : 
                            NcbiHelper.GetFileStream(commonSequence.RemoteId);

                        switch (Db.Notation.Single(m => m.Id == commonSequence.NotationId).Nature)
                        {
                            case Nature.Genetic:
                                dnaSequenceRepository.Create(commonSequence, sequenceStream, partial ?? false);
                                break;
                            case Nature.Music:
                                musicSequenceRepository.Create(commonSequence, sequenceStream);
                                break;
                            case Nature.Literature:
                                literatureSequenceRepository.Create(commonSequence, sequenceStream, languageId ?? 0, original ?? false, translatorId);
                                break;
                            case Nature.MeasurementData:
                                dataSequenceRepository.Create(commonSequence, sequenceStream, precision ?? 0);
                                break;
                            default:
                                throw new Exception("Unknown nature.");
                        }

                        return new Dictionary<string, object>
                        {
                            { "matterName", commonSequence.Matter.Name }
                        };
                    }

                    var translators = new SelectList(Db.Translator, "id", "name").ToList();
                    translators.Add(new SelectListItem { Value = null, Text = "Нет" });

                    var remoteDbs = commonSequence.RemoteDbId.HasValue
                        ? remoteDbRepository.GetSelectListWithNature(commonSequence.RemoteDbId.Value)
                        : remoteDbRepository.GetSelectListWithNature();

                    var sequenceNature = Db.Notation.Single(m => m.Id == commonSequence.NotationId).Nature;

                    IEnumerable<SelectListItem> natures;
                    IEnumerable<object> notations;
                    IEnumerable<object> features;

                    if (UserHelper.IsAdmin())
                    {
                        natures = EnumHelper.GetSelectList(typeof(Nature), sequenceNature);
                        notations = notationRepository.GetSelectListWithNature(commonSequence.NotationId);
                        features = featureRepository.GetSelectListWithNature();
                    }
                    else
                    {
                        natures = new List<Nature> { Nature.Genetic }.ToSelectList(sequenceNature);
                        notations = notationRepository.GetSelectListWithNature(new List<int> { Aliases.Notation.Nucleotide }, commonSequence.NotationId);
                        features = featureRepository.GetSelectListWithNature(new List<int> { Aliases.Feature.FullGenome, Aliases.Feature.RibosomalRNA }, commonSequence.FeatureId);
                    }

                    var result = new Dictionary<string, object>
                    {
                        { "ErrorMessage", errorMessage },
                        { "matters", matterRepository.GetMatterSelectList(commonSequence.MatterId) },
                        { "natures", natures },
                        { "notations", notations },
                        { "features", features },
                        { "languages", new SelectList(Db.Language, "id", "name", languageId) },
                        { "remoteDbs", remoteDbs },
                        { "nature", (byte)sequenceNature },
                        { "translators", translators },
                        { "natureLiterature", (byte)Nature.Literature },
                        { "commonSequence", commonSequence }
                    };

                    return result;
                }
                catch (Exception)
                {
                    var orphanMatter = Db.Matter.Include(m => m.Sequence).Where(m => m.Name == commonSequence.Matter.Name && m.Sequence.Count == 0).ToList();

                    if (orphanMatter.Count > 0)
                    {
                        Db.Matter.Remove(orphanMatter[0]);
                        Db.SaveChanges();
                    }

                    throw;
                }
                finally
                {
                    ThreadDisposable = true;
                    Dispose(true);
                }
            });
        }

        /// <summary>
        /// The result.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public override ActionResult Result()
        {
            try
            {
                var result = TempData["result"] as Dictionary<string, object>;
                if (result == null)
                {
                    throw new Exception("No data.");
                }

                foreach (var key in result.Keys)
                {
                    ViewData[key] = result[key];
                }

                TempData.Keep();

                if (!string.IsNullOrEmpty((ViewData["ErrorMessage"] ?? string.Empty).ToString()))
                {
                    RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);

                ViewBag.Error = true;

                ViewBag.ErrorMessage = e.Message;
            }

            return View();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && ThreadDisposable)
            {
                Db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
