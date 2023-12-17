namespace Libiada.Web.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.EntityFrameworkCore;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using Bio;

    using LibiadaCore.Extensions;

    using Libiada.Web.Extensions;
    using Libiada.Database.Helpers;
    using Libiada.Database.Models.Repositories.Sequences;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;
    using Libiada.Web.Helpers;
    using FileHelper = Helpers.FileHelper;
    using Libiada.Web.Tasks;

    /// <summary>
    /// The sequences matters controller.
    /// </summary>
    public abstract class SequencesMattersController : AbstractResultController
    {
        protected readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;
        private readonly Cache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesMattersController"/> class.
        /// </summary>
        /// <param name="taskType">
        /// The task Type.
        /// </param>
        protected SequencesMattersController(TaskType taskType,
                                             LibiadaDatabaseEntities db,
                                             IViewDataHelper viewDataHelper,
                                             ITaskManager taskManager,
                                             Cache cache)
            : base(taskType, taskManager)
        {
            this.db = db;
            this.viewDataHelper = viewDataHelper;
            this.cache = cache;
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillMatterCreationViewData());
            return View();
        }

        /// <summary>
        /// Sequence creation method.
        /// </summary>
        /// <param name="commonSequence">
        /// The sequence.
        /// </param>
        /// <param name="localFile">
        /// The local file.
        /// </param>
        /// <param name="file">
        /// Sequence file as <see cref="IFormFile"/>.
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
        public ActionResult Create(//[Bind(Include = "Id,Notation,RemoteDb,RemoteId,Description,Matter,MatterId")] 
            CommonSequence commonSequence,
            bool localFile,
            IFormFile? file,
            Language? language,
            bool? original,
            Translator? translator,
            bool? partial,
            int? precision)
        {
            return CreateTask(() =>
            {
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
                        sequenceStream = FileHelper.GetFileStream(file!);
                    }

                    switch (nature)
                    {
                        case Nature.Genetic:
                            ISequence bioSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                            var dnaSequenceRepository = new GeneticSequenceRepository(db, cache);
                            dnaSequenceRepository.Create(commonSequence, bioSequence, partial ?? false);
                            break;
                        case Nature.Music:
                            var musicSequenceRepository = new MusicSequenceRepository(db, cache);
                            musicSequenceRepository.Create(commonSequence, sequenceStream);
                            break;
                        case Nature.Literature:
                            var literatureSequenceRepository = new LiteratureSequenceRepository(db, cache);
                            literatureSequenceRepository.Create(commonSequence, sequenceStream, language ?? Language.Russian, original ?? true, translator ?? Translator.NoneOrManual);
                            break;
                        case Nature.MeasurementData:
                            var dataSequenceRepository = new DataSequenceRepository(db, cache);
                            dataSequenceRepository.Create(commonSequence, sequenceStream, precision ?? 0);
                            break;
                        case Nature.Image:
                            var matterRepository = new MatterRepository(db, cache);

                            byte[] fileBytes;
                            using (Stream fileStream = FileHelper.GetFileStream(file))
                            {
                                fileBytes = new byte[fileStream.Length];
                                fileStream.Read(fileBytes, 0, (int)fileStream.Length);
                            }

                            var matter = new Matter
                            {
                                Nature = Nature.Image,
                                SequenceType = commonSequence.Matter.SequenceType,
                                Name = commonSequence.Matter.Name,
                                Source = fileBytes,
                                Group = commonSequence.Matter.Group
                            };
                            matterRepository.SaveToDatabase(matter);
                            break;
                        default:
                            throw new InvalidEnumArgumentException(nameof(nature), (int)nature, typeof(Nature));
                    }
                    string? multisequenceName = db.Multisequences.SingleOrDefault(ms => ms.Id == commonSequence.Matter.MultisequenceId)?.Name;
                    var result = new ImportResult(commonSequence, language, original, translator, partial, precision, multisequenceName);

                    return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
                }
                catch (Exception)
                {
                    long matterId = commonSequence.MatterId;
                    if (matterId != 0)
                    {
                        List<Matter> orphanMatter = db.Matters
                            .Include(m => m.Sequence)
                            .Where(m => m.Id == matterId && m.Sequence.Count == 0)
                            .ToList();

                        if (orphanMatter.Count > 0)
                        {
                            db.Matters.Remove(orphanMatter[0]);
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
        private readonly struct ImportResult
        {
            /// <summary>
            /// The name.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// The description.
            /// </summary>
            public readonly string? Description;

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
            public readonly string? RemoteId;

            /// <summary>
            /// The language.
            /// </summary>
            public readonly string? Language;

            /// <summary>
            /// The original.
            /// </summary>
            public readonly bool? Original;

            /// <summary>
            /// The translator.
            /// </summary>
            public readonly string? Translator;

            /// <summary>
            /// The partial.
            /// </summary>
            public readonly bool? Partial;

            /// <summary>
            /// The precision.
            /// </summary>
            public readonly double? Precision;

            public readonly string? MultisequenceName;

            public readonly int? MultisequenceNumber;

            public readonly string? CollectionCountry;

            public readonly DateOnly? CollectionDate;

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
            /// <param name="multisequenceName">
            /// The multisequence name.
            /// </param>
            public ImportResult(
                CommonSequence sequence,
                Language? language,
                bool? original,
                Translator? translator,
                bool? partial,
                double? precision,
                string? multisequenceName)
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
                MultisequenceName = multisequenceName;
                MultisequenceNumber = matter.MultisequenceNumber;
                CollectionCountry = matter.CollectionCountry;
                CollectionDate = matter.CollectionDate;
            }
        }
    }
}
