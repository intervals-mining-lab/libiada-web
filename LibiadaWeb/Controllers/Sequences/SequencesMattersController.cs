namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
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
        /// Initializes a new instance of the <see cref="SequencesMattersController"/> class.
        /// </summary>
        protected SequencesMattersController() : base("Sequence upload")
        {
            Db = new LibiadaWebEntities();
            dnaSequenceRepository = new DnaSequenceRepository(Db);
            literatureSequenceRepository = new LiteratureSequenceRepository(Db);
            musicSequenceRepository = new MusicSequenceRepository(Db);
            dataSequenceRepository = new DataSequenceRepository(Db);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            var viewDataHelper = new ViewDataHelper(Db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillMatterCreationData());

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
            [Bind(Include = "Id,NotationId,FeatureId,RemoteDbId,RemoteId,Description,Matter")] CommonSequence commonSequence,
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
                    if (!ModelState.IsValid)
                    {
                        throw new Exception("Model state is invalid");
                    }

                    Stream sequenceStream;
                    var nature = Db.Notation.Single(m => m.Id == commonSequence.NotationId).Nature;
                    if (nature == Nature.Genetic && !localFile)
                    {
                        sequenceStream = NcbiHelper.GetFastaFileStream(commonSequence.RemoteId);
                    }
                    else
                    {
                        sequenceStream = FileHelper.GetFileStream(Request.Files[0]);
                    }

                    switch (Db.Notation.Single(m => m.Id == commonSequence.NotationId).Nature)
                    {
                        case Nature.Genetic:
                            var bioSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                            dnaSequenceRepository.Create(commonSequence, bioSequence, partial ?? false);
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
        /// <param name="taskId">
        /// The task Id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public override ActionResult Result(string taskId)
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
        /// The disposing flag.
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
