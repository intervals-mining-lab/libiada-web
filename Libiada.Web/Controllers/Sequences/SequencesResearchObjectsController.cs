namespace Libiada.Web.Controllers.Sequences;

using System.ComponentModel;

using Libiada.Core.Extensions;

using Libiada.Database.Helpers;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Helpers;
using Libiada.Web.Extensions;
using Libiada.Web.Tasks;

using FileHelper = Helpers.FileHelper;

/// <summary>
/// The sequences and research objects controller.
/// </summary>
public abstract class SequencesResearchObjectsController : AbstractResultController
{
    protected readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataBuilder viewDataBuilder;
    private readonly INcbiHelper ncbiHelper;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequencesResearchObjectsController"/> class.
    /// </summary>
    /// <param name="taskType">
    /// The task Type.
    /// </param>
    protected SequencesResearchObjectsController(TaskType taskType,
                                         IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                         IViewDataBuilder viewDataBuilder,
                                         ITaskManager taskManager,
                                         INcbiHelper ncbiHelper,
                                         IResearchObjectsCache cache)
        : base(taskType, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataBuilder = viewDataBuilder;
        this.ncbiHelper = ncbiHelper;
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
        var viewData = viewDataBuilder.AddResearchObjects()
                                      .AddNatures()
                                      .AddNotations()
                                      .AddRemoteDatabases()
                                      .AddSequenceTypes()
                                      .AddGroups()
                                      .AddMultisequences()
                                      .AddLanguages()
                                      .AddTranslators()
                                      .AddTrajectories()
                                      .Build();
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// Sequence creation method.
    /// </summary>
    /// <param name="sequence">
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
    public ActionResult Create(CombinedSequenceEntity sequence, bool localFile, IFormFile? file, int? precision)
    {
        int userId = User.GetUserId();
        sequence.CreatorId = userId;
        sequence.ModifierId = userId;

        return CreateTask(() =>
        {
            Stream? sequenceStream = null;
            try
            {
                // Remove alphabet and order from validation 
                // if they are going to be filled from file or external source 
                if ((localFile && file != null) || sequence.RemoteId != null)
                {
                    ModelState.Remove("Alphabet");
                    ModelState.Remove("Order");
                }

                if (!ModelState.IsValid)
                {
                    throw new Exception("Model state is invalid");
                }

                Nature nature = sequence.Notation.GetNature();
                if (nature == Nature.Genetic && !localFile)
                {
                    sequenceStream = ncbiHelper.GetFastaFileStream(sequence.RemoteId ?? throw new Exception("No remote id is provided for genetic sequence to import"));
                }
                else
                {
                    sequenceStream = FileHelper.GetFileStream(file ?? throw new Exception("No file with sequence is provided for import"));
                }

                using var db = dbFactory.CreateDbContext();

                switch (nature)
                {
                    case Nature.Genetic:
                        Bio.ISequence bioSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                        using (var geneticSequenceRepository = new GeneticSequenceRepository(dbFactory, cache))
                        {
                            var geneticSequence = sequence.ToGeneticSequence();

                            geneticSequenceRepository.Create(geneticSequence, bioSequence);
                        }

                        break;
                    case Nature.Music:
                        var musicSequenceRepository = new MusicSequenceRepository(dbFactory, cache);
                        var musicSequence = sequence.ToMusicSequence();

                        // TODO: deside if this method should create only one music sequence type or all of them 
                        musicSequenceRepository.Create(musicSequence, sequenceStream);
                        break;
                    case Nature.Literature:
                        var literatureSequenceRepository = new LiteratureSequenceRepository(dbFactory, cache);
                        var literatureSequence = sequence.ToLiteratureSequence(); 

                        literatureSequenceRepository.Create(literatureSequence, sequenceStream);
                        break;
                    case Nature.MeasurementData:
                        var dataSequenceRepository = new DataSequenceRepository(dbFactory, cache);
                        var dataSequence = new DataSequence
                        {
                            CreatorId = userId,
                            ModifierId = userId,
                            Notation = sequence.Notation,
                            RemoteDb = sequence.RemoteDb,
                            RemoteId = sequence.RemoteId,
                            ResearchObject = sequence.ResearchObject
                        };
                        dataSequenceRepository.Create(dataSequence, sequenceStream, precision ?? 0);
                        break;
                    case Nature.Image:
                        var researchObjectRepository = new ResearchObjectRepository(db, cache);

                        byte[] fileBytes;
                        
                        fileBytes = new byte[sequenceStream.Length];
                        sequenceStream.Read(fileBytes, 0, (int)sequenceStream.Length);

                        var researchObject = new ResearchObject
                        {
                            Nature = Nature.Image,
                            SequenceType = sequence.ResearchObject.SequenceType,
                            Name = sequence.ResearchObject.Name,
                            Source = fileBytes,
                            Group = sequence.ResearchObject.Group
                        };
                        researchObjectRepository.SaveToDatabase(researchObject);
                        break;
                    default:
                        throw new InvalidEnumArgumentException(nameof(nature), (int)nature, typeof(Nature));
                }

                string? multisequenceName = db.Multisequences.SingleOrDefault(ms => ms.Id == sequence.ResearchObject.MultisequenceId)?.Name;
                var result = new ImportResult(sequence, precision, multisequenceName);

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            }
            catch (Exception)
            {
                using var db = dbFactory.CreateDbContext();
                long researchObjectId = sequence.ResearchObjectId;
                if (researchObjectId != 0)
                {
                    List<ResearchObject> orphanResearchObject = db.ResearchObjects
                        .Include(m => m.Sequences)
                        .Where(m => m.Id == researchObjectId && m.Sequences.Count == 0)
                        .ToList();

                    if (orphanResearchObject.Count > 0)
                    {
                        db.ResearchObjects.Remove(orphanResearchObject[0]);
                        db.SaveChanges();
                    }
                }

                throw;
            }
            finally
            {
                sequenceStream?.Dispose();
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
            CombinedSequenceEntity sequence,
            double? precision,
            string? multisequenceName)
        {
            ResearchObject researchObject = sequence.ResearchObject;

            Name = researchObject.Name;
            Description = researchObject.Description;
            Nature = researchObject.Nature.GetDisplayValue();
            Notation = sequence.Notation.GetDisplayValue();
            Group = researchObject.Group.GetDisplayValue();
            SequenceType = researchObject.SequenceType.GetDisplayValue();
            RemoteId = sequence.RemoteId;
            Language = sequence.Language?.GetDisplayValue();
            Original = sequence.Original;
            Translator = sequence.Translator?.GetDisplayValue();
            Partial = sequence.Partial;
            Precision = precision;
            MultisequenceName = multisequenceName;
            MultisequenceNumber = researchObject.MultisequenceNumber;
            CollectionCountry = researchObject.CollectionCountry;
            CollectionDate = researchObject.CollectionDate;
        }
    }
}
