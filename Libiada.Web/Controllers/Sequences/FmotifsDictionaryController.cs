namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Core.SimpleTypes;

using Libiada.Database.Tasks;
using Libiada.Database.Helpers;

using Newtonsoft.Json;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

/// <summary>
/// The Fmotifs dictionary controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class FmotifsDictionaryController : SequencesMattersController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FmotifsDictionaryController"/> class.
    /// </summary>
    public FmotifsDictionaryController(ILibiadaDatabaseEntitiesFactory dbFactory, 
                                       IViewDataHelper viewDataHelper, 
                                       ITaskManager taskManager,
                                       INcbiHelper ncbiHelper,
                                       Cache cache) 
        : base(TaskType.FmotifsDictionary, dbFactory, viewDataHelper, taskManager,ncbiHelper, cache)
    {
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    public async Task<ActionResult> Index()
    {
        var musicSequence = dbFactory.CreateDbContext().MusicSequences.Where(m => m.Notation == Notation.FormalMotifs).Include(m => m.Matter);
        return View(await musicSequence.ToListAsync());

    }

    /// <summary>
    /// The details.
    /// </summary>
    /// <param name="id">
    /// The music sequence's id.
    /// </param>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    public async Task<ActionResult> Details(long? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        var db = dbFactory.CreateDbContext();
        MusicSequence musicSequence = await db.MusicSequences.Include(m => m.Matter).SingleAsync(m => m.Id == id);
        if (musicSequence == null)
        {
            return NotFound();
        }

        var musicChainAlphabet = musicSequence.Alphabet.Select(el => db.Fmotifs.Single(f => f.Id == el)).ToList();
        var sortedFmotifs = new Dictionary<Database.Models.Fmotif, int>();
        for (int i = 0; i < musicChainAlphabet.Count; i++)
        {
            sortedFmotifs.Add(musicChainAlphabet[i], musicSequence.Order.Count(el => el == i + 1));
        }

        sortedFmotifs = sortedFmotifs.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

        var fmotifsChain = new List<Fmotif>();
        foreach (var fmotif in sortedFmotifs.Keys)
        {
            var newFmotif = new Fmotif(fmotif.FmotifType, musicSequence.PauseTreatment, fmotif.Id);

            var fmotifAlphabet = fmotif.Alphabet;
            var fmotifOrder = fmotif.Order;
            foreach (var position in fmotifOrder)
            {
                var dbNoteId = fmotifAlphabet[position - 1];
                var dbNote = await db.Notes.Include(n => n.Pitches).SingleAsync(n => n.Id == dbNoteId);
                var newPitches = new List<Pitch>();
                foreach (var pitch in dbNote.Pitches)
                {
                    newPitches.Add(new Pitch(pitch.Midinumber));
                }

                var newNote = new ValueNote(newPitches,
                                            new Duration(dbNote.Numerator, dbNote.Denominator),
                                            dbNote.Triplet,
                                            dbNote.Tie);
                newNote.Id = dbNote.Id;
                newFmotif.NoteList.Add(newNote);
            }

            fmotifsChain.Add(newFmotif);
        }

        var result = new Dictionary<string, object>
        {
            { "fmotifs", fmotifsChain },
            { "sequentialTransfer", musicSequence.SequentialTransfer }
        };
        ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object> { { "data", result } });
        return View(musicSequence);
    }
}
