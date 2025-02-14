namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Core;
using Libiada.Core.Music;

using Libiada.Database.Models;
using Libiada.Database.Models.Repositories.Sequences;

using Libiada.Web.Helpers;
using Libiada.Web.Extensions;

using Newtonsoft.Json;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The sequence mixer controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequenceMixerController : Controller
{
    /// <summary>
    /// Database context factory.
    /// </summary>
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;

    private readonly ResearchObjectRepository researchObjectRepository;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly CombinedSequenceEntityRepository sequenceRepository;

    /// <summary>
    /// The element repository.
    /// </summary>
    private readonly ElementRepository elementRepository;

    /// <summary>
    /// The random generator.
    /// </summary>
    private readonly Random randomGenerator = new();
    private readonly IViewDataHelper viewDataHelper;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceMixerController"/> class.
    /// </summary>
    public SequenceMixerController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                   IViewDataHelper viewDataHelper,
                                   IResearchObjectsCache cache)
    {
        this.dbFactory = dbFactory;
        researchObjectRepository = new ResearchObjectRepository(dbFactory.CreateDbContext(), cache);
        sequenceRepository = new CombinedSequenceEntityRepository(dbFactory, cache);
        elementRepository = new ElementRepository(dbFactory.CreateDbContext());
        this.viewDataHelper = viewDataHelper;
        this.cache = cache;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        var viewData = viewDataHelper.AddMinMaxResearchObjects(1, 1)
                                     .AddNatures()
                                     .AddNotations()
                                     .AddLanguages()
                                     .AddTranslators()
                                     .AddPauseTreatments()
                                     .AddTrajectories()
                                     .AddSequenceTypes()
                                     .AddGroups()
                                     .AddSubmitName("Mix")
                                     .Build();
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectId">
    /// The research object id.
    /// </param>
    /// <param name="notation">
    /// The notation id.
    /// </param>
    /// <param name="language">
    /// The language id.
    /// </param>
    /// <param name="translator">
    /// The translator id.
    /// </param>
    /// <param name="scrambling">
    /// The mixes.
    /// </param
    /// <param name="pauseTreatment">
    /// Pause treatment parameters of music sequences.
    /// </param>
    /// <param name="sequentialTransfer">
    /// Sequential transfer flag used in music sequences.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if sequence nature is unknown.
    /// </exception>
    [HttpPost]
    public ActionResult Index(long researchObjectId,
                              Notation notation,
                              Language? language,
                              Translator? translator,
                              PauseTreatment? pauseTreatment,
                              bool? sequentialTransfer,
                              int scrambling)
    {

        using var db = dbFactory.CreateDbContext();
        ResearchObject researchObject = cache.ResearchObjects.Single(m => m.Id == researchObjectId);
        long sequenceId = researchObject.Nature switch
        {
            Nature.Literature => db.CombinedSequenceEntities.Single(l => l.ResearchObjectId == researchObjectId
                                                                      && l.Notation == notation
                                                                      && l.Language == language
                                                                      && l.Translator == translator).Id,
            Nature.Music => db.CombinedSequenceEntities.Single(m => m.ResearchObjectId == researchObjectId
                                                                 && m.Notation == notation
                                                                 && m.PauseTreatment == pauseTreatment
                                                                 && m.SequentialTransfer == sequentialTransfer).Id,
            _ => db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == researchObjectId && c.Notation == notation).Id,
        };
        Sequence sequence = sequenceRepository.GetLibiadaSequence(sequenceId);
        for (int i = 0; i < scrambling; i++)
        {
            int firstIndex = randomGenerator.Next(sequence.Length);
            int secondIndex = randomGenerator.Next(sequence.Length);

            IBaseObject firstElement = sequence[firstIndex];
            IBaseObject secondElement = sequence[secondIndex];
            sequence[firstIndex] = secondElement;
            sequence[secondIndex] = firstElement;
        }

        var resultResearchObject = new ResearchObject
        {
            Nature = researchObject.Nature,
            Name = $"{researchObject.Name} {scrambling} mixes"
        };

        researchObjectRepository.SaveToDatabase(resultResearchObject);

        long[] alphabet = elementRepository.ToDbElements(sequence.Alphabet, notation, false);

        CombinedSequenceEntity dbSequence = db.CombinedSequenceEntities.Single(c => c.Id == sequenceId);

        var newSequence = new CombinedSequenceEntity
        {
            Notation = notation,
            ResearchObjectId = resultResearchObject.Id,
            Alphabet = alphabet,
            Order = sequence.Order,
            PauseTreatment = dbSequence.PauseTreatment,
            SequentialTransfer = dbSequence.SequentialTransfer,
            Language = dbSequence.Language,
            Original = dbSequence.Original,
            Translator = dbSequence.Translator,
            Partial = dbSequence.Partial,
            Creator = dbSequence.Creator,
            ModifierId = User.GetUserId()
        };

        sequenceRepository.Create(newSequence);

        return RedirectToAction("Index", "ResearchObjects");
    }
}
