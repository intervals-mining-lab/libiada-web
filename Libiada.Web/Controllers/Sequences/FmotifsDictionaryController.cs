﻿namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Core.SimpleTypes;

using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;

/// <summary>
/// The Fmotifs dictionary controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class FmotifsDictionaryController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FmotifsDictionaryController"/> class.
    /// </summary>
    public FmotifsDictionaryController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                       ITaskManager taskManager)
        : base(TaskType.FmotifsDictionary, taskManager)
    {
        this.dbFactory = dbFactory;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    public async Task<ActionResult> Index()
    {
        using var db = dbFactory.CreateDbContext();
        var musicSequences = db.CombinedSequenceEntities
                               .Where(m => m.Notation == Notation.FormalMotifs)
                               .Include(m => m.ResearchObject)
                               .Select(s => s.ToMusicSequence());
        return View(await musicSequences.ToListAsync());

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

        using var db = dbFactory.CreateDbContext();
        var dbSequence = await db.CombinedSequenceEntities
                                 .Include(m => m.ResearchObject)
                                 .SingleAsync(m => m.Id == id);

        if (dbSequence == null)
        {
            return NotFound();
        }

        var musicSequence = dbSequence.ToMusicSequence();

        var musicSequenceAlphabet = musicSequence.Alphabet
                                                 .Select(el => db.Fmotifs.Single(f => f.Id == el))
                                                 .ToList();
        var sortedFmotifs = new Dictionary<Database.Models.Fmotif, int>();
        for (int i = 0; i < musicSequenceAlphabet.Count; i++)
        {
            sortedFmotifs.Add(musicSequenceAlphabet[i], musicSequence.Order.Count(el => el == i + 1));
        }

        //TODO: check if orderBY is necessary
        sortedFmotifs = sortedFmotifs.OrderByDescending(pair => pair.Value)
                                     .ToDictionary(pair => pair.Key, pair => pair.Value);

        List<Fmotif> fmotifsSequence = [];
        List<int> fmotifsCounts = [];
        foreach (var fmotif in sortedFmotifs.Keys)
        {
            var newFmotif = new Fmotif(fmotif.FmotifType, musicSequence.PauseTreatment, fmotif.Id);

            long[] fmotifAlphabet = fmotif.Alphabet;
            int[] fmotifOrder = fmotif.Order;
            foreach (int position in fmotifOrder)
            {
                long dbNoteId = fmotifAlphabet[position - 1];
                Note dbNote = await db.Notes.Include(n => n.Pitches).SingleAsync(n => n.Id == dbNoteId);
                List<Pitch> newPitches = [];
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

            fmotifsSequence.Add(newFmotif);
        }

        foreach(var fmotifCount in sortedFmotifs.Values)
        {
            fmotifsCounts.Add(fmotifCount);
        }

        var result = new Dictionary<string, object>
        {
            { "fmotifs", fmotifsSequence },
            { "fmotifsCounts", fmotifsCounts },
            { "sequentialTransfer", musicSequence.SequentialTransfer }
        };
        ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object> { { "data", result } });
        return View(musicSequence);
    }

    /// <summary>
    /// Загружает нотную последовательность всего произведения.
    /// </summary>
    /// <param name="id">ID музыкального произведения.</param>
    /// <returns>Представление нотного стана для всего произведения.</returns>
    public async Task<ActionResult> MusicScore(long? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        using var db = dbFactory.CreateDbContext();
        var dbSequence = await db.CombinedSequenceEntities
                                 .Where(m => m.Notation == Notation.Measures)
                                 .Include(m => m.ResearchObject)
                                 .SingleOrDefaultAsync(m => m.ResearchObjectId == id);

        if (dbSequence == null)
        {
            return NotFound("Ошибка БД: не найдена подходящая запись.");
        }

        var musicSequence = dbSequence.ToMusicSequence();

        var musicSequenceAlphabet = musicSequence.Alphabet
                                                 .Select(el => db.Measures.Single(f => f.Id == el))
                                                 .ToList();

        List<ValueNote> notesSequence = new List<ValueNote>();
        List<List<ValueNote>> measuresSequence = new List<List<ValueNote>>();

        foreach (var measure in musicSequenceAlphabet)
        {
            List<ValueNote> measureNotes = new List<ValueNote>();
            long[] measureAlphabet = measure.Alphabet;
            int[] measureOrder = measure.Order;

            foreach (int position in measureOrder)
            {
                long dbNoteId = measureAlphabet[position - 1];
                Note dbNote = await db.Notes.Include(n => n.Pitches).SingleAsync(n => n.Id == dbNoteId);

                List<Pitch> newPitches = dbNote.Pitches.Select(pitch => new Pitch(pitch.Midinumber)).ToList();
                var newNote = new ValueNote(newPitches,
                                            new Duration(dbNote.Numerator, dbNote.Denominator),
                                            dbNote.Triplet,
                                            dbNote.Tie)
                {
                    Id = dbNote.Id
                };

                notesSequence.Add(newNote);
                measureNotes.Add(newNote);
            }
            measuresSequence.Add(measureNotes);
        }

        var result = new Dictionary<string, object>
    {
        { "musicNotes", notesSequence },
        { "measures", measuresSequence },
        { "sequentialTransfer", musicSequence.SequentialTransfer }
    };

        ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object> { { "data", result } });
        return View(musicSequence);
    }




}
