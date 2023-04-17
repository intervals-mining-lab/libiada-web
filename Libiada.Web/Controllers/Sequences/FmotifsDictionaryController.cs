namespace Libiada.Web.Controllers.Sequences
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    using LibiadaCore.Core.SimpleTypes;

    using Libiada.Database.Tasks;
    using Libiada.Database.Extensions;

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
        public FmotifsDictionaryController(LibiadaDatabaseEntities db, 
                                           IViewDataHelper viewDataHelper, 
                                           ITaskManager taskManager, 
                                           Cache cache) 
            : base(TaskType.FmotifsDictionary, db, viewDataHelper, taskManager, cache)
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
            var musicSequence = db.MusicSequence.Where(m => m.Notation == Notation.FormalMotifs).Include(m => m.Matter);
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

            MusicSequence musicSequence = db.MusicSequence.Include(m => m.Matter).Single(m => m.Id == id);
            if (musicSequence == null)
            {
                return NotFound();
            }

            var musicChainAlphabet = db.GetAlphabetElementIds(musicSequence.Id)
                                                       .Select(el => db.Fmotif.Single(f => f.Id == el))
                                                       .ToList();
            var musicChainBuilding = db.GetSequenceBuilding(musicSequence.Id);
            var sortedFmotifs = new Dictionary<Libiada.Database.Fmotif, int>();
            for (int i = 0; i < musicChainAlphabet.Count; i++)
            {
                sortedFmotifs.Add(musicChainAlphabet[i], musicChainBuilding.Count(el => el == i + 1));
            }

            sortedFmotifs = sortedFmotifs.OrderByDescending(pair => pair.Value)
                                         .ToDictionary(pair => pair.Key, pair => pair.Value);

            var fmotifsChain = new List<Fmotif>();
            foreach (var fmotif in sortedFmotifs.Keys)
            {
                var newFmotif = new Fmotif(fmotif.FmotifType, musicSequence.PauseTreatment, fmotif.Id);

                var fmotifAlphabet = db.GetFmotifAlphabet(fmotif.Id);
                var fmotifBuilding = db.GetFmotifBuilding(fmotif.Id);
                foreach (var position in fmotifBuilding)
                {
                    var dbNoteId = fmotifAlphabet.ElementAt(position - 1);
                    var dbNote = db.Note.Single(n => n.Id == dbNoteId);
                    var newPitches = new List<Pitch>();
                    foreach (var pitch in dbNote.Pitch)
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

}
