namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using Bio;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The sequences matters controller.
    /// </summary>
    public abstract class SequencesMattersController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesMattersController"/> class.
        /// </summary>
        /// <param name="taskType">
        /// The task Type.
        /// </param>
        protected SequencesMattersController(TaskType taskType) : base(taskType)
        {
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillMatterCreationData());
            }

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
        /// <param name="language">
        /// The language id.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="translator">
        /// The translator id.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <param name="precision">
        /// Precision of data sequence.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Id,Notation,RemoteDb,RemoteId,Description,Matter,MatterId")] CommonSequence commonSequence,
            bool localFile,
            Language? language,
            bool? original,
            Translator? translator,
            bool? partial,
            int? precision)
        {
            return Action(() =>
            {
                var db = new LibiadaWebEntities();
                try
                {
                    if (!ModelState.IsValid)
                    {
                        throw new Exception("Model state is invalid");
                    }

                    Stream sequenceStream;
                    Nature nature = commonSequence.Notation.GetNature();
                    if (nature == Nature.Genetic && !localFile)
                    {
                        sequenceStream = NcbiHelper.GetFastaFileStream(commonSequence.RemoteId);
                    }
                    else
                    {
                        sequenceStream = FileHelper.GetFileStream(Request.Files[0]);
                    }

                    var dnaSequenceRepository = new DnaSequenceRepository(db);
                    var literatureSequenceRepository = new LiteratureSequenceRepository(db);
                    var musicSequenceRepository = new MusicSequenceRepository(db);
                    var dataSequenceRepository = new DataSequenceRepository(db);

                    switch (nature)
                    {
                        case Nature.Genetic:
                            ISequence bioSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                            dnaSequenceRepository.Create(commonSequence, bioSequence, partial ?? false);
                            break;
                        case Nature.Music:
                            musicSequenceRepository.Create(commonSequence, sequenceStream);
                            break;
                        case Nature.Literature:
                            literatureSequenceRepository.Create(commonSequence, sequenceStream, language ?? Language.Russian, original ?? true, translator ?? Translator.NoneOrManual);
                            break;
                        case Nature.MeasurementData:
                            dataSequenceRepository.Create(commonSequence, sequenceStream, precision ?? 0);
                            break;
                        default:
                            throw new InvalidEnumArgumentException(nameof(nature), (int)nature, typeof(Nature));
                    }

                    var data = new ImportResult(commonSequence, language, original, translator, partial, precision);
                    return new Dictionary<string, object>
                               {
                                   { "data", JsonConvert.SerializeObject(data) }
                               };
                }
                catch (Exception)
                {
                    long matterId = commonSequence.MatterId;
                    if (matterId != 0)
                    {
                        List<Matter> orphanMatter = db.Matter
                            .Include(m => m.Sequence)
                            .Where(m => m.Id == matterId && m.Sequence.Count == 0)
                            .ToList();

                        if (orphanMatter.Count > 0)
                        {
                            db.Matter.Remove(orphanMatter[0]);
                            db.SaveChanges();
                        }
                    }

                    throw;
                }
                finally
                {
                    Dispose(true);
                }
            });
        }

        /// <summary>
        /// Sequence import result struct.
        /// </summary>
        private struct ImportResult
        {
            /// <summary>
            /// The name.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// The description.
            /// </summary>
            public readonly string Description;

            /// <summary>
            /// The nature.
            /// </summary>
            public readonly string Nature;

            /// <summary>
            /// The notation of the imported sequence.
            /// </summary>
            public readonly string Notation;

            /// <summary>
            /// The group.
            /// </summary>
            public readonly string Group;

            /// <summary>
            /// The sequence type.
            /// </summary>
            public readonly string SequenceType;

            /// <summary>
            /// The remote id.
            /// </summary>
            public readonly string RemoteId;

            /// <summary>
            /// The language.
            /// </summary>
            public readonly string Language;

            /// <summary>
            /// The original.
            /// </summary>
            public readonly bool? Original;

            /// <summary>
            /// The translator.
            /// </summary>
            public readonly string Translator;

            /// <summary>
            /// The partial.
            /// </summary>
            public readonly bool? Partial;

            /// <summary>
            /// The precision.
            /// </summary>
            public readonly double? Precision;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImportResult"/> struct.
            /// </summary>
            /// <param name="sequence">
            /// The sequence.
            /// </param>
            /// <param name="language">
            /// The language.
            /// </param>
            /// <param name="original">
            /// The original.
            /// </param>
            /// <param name="translator">
            /// The translator.
            /// </param>
            /// <param name="partial">
            /// The partial.
            /// </param>
            /// <param name="precision">
            /// The precision.
            /// </param>
            public ImportResult(
                CommonSequence sequence,
                Language? language,
                bool? original,
                Translator? translator,
                bool? partial,
                double? precision)
            {
                Matter matter = sequence.Matter;

                Name = matter.Name;
                Description = matter.Description;
                Nature = matter.Nature.GetDisplayValue();
                Notation = sequence.Notation.GetDisplayValue();
                Group = matter.Group.GetDisplayValue();
                SequenceType = matter.SequenceType.GetDisplayValue();
                RemoteId = sequence.RemoteId;
                Language = language?.GetDisplayValue();
                Original = original;
                Translator = translator?.GetDisplayValue();
                Partial = partial;
                Precision = precision;
            }
        }
    }
}
