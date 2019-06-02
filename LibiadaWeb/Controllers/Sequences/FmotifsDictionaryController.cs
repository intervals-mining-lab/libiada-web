using Bio.Util;

namespace LibiadaWeb.Controllers.Sequences
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Collections.Generic;

    using LibiadaWeb.Tasks;
    using LibiadaWeb.Helpers;
    using Newtonsoft.Json;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaCore.Music;

    /// <summary>
    /// The Fmotifs dictionary controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class FmotifsDictionaryController: SequencesMattersController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FmotifsDictionaryController"/> class.
        /// </summary>
        public FmotifsDictionaryController() : base(TaskType.FmotifsDictionary)
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
            using (var db = new LibiadaWebEntities())
            {
                var musicSequence = db.MusicSequence.Where(m => m.Notation == Notation.FormalMotifs).Include(m => m.Matter);
                return View(await musicSequence.ToListAsync());
            }
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new LibiadaWebEntities())
            {
                MusicSequence musicSequence = db.MusicSequence.Include(m => m.Matter).Single(m => m.Id == id);
                if (musicSequence == null)
                {
                    return HttpNotFound();
                }

                var musicChainAlphabet = DbHelper.GetMusicChainAlphabet(db, musicSequence.Id).Select(el => db.Fmotif.Single(f => f.Id == el)).ToList();
                var musicChainBuilding = DbHelper.GetMusicChainBuilding(db, musicSequence.Id);
                var sortedFmotifs = new Dictionary<LibiadaWeb.Fmotif, int>();
                for (int i = 0; i < musicChainAlphabet.Count; i++)
                {
                    sortedFmotifs.Add(musicChainAlphabet[i], musicChainBuilding.Count(el => el == i + 1));
                }
                sortedFmotifs = sortedFmotifs.OrderByDescending(pair => pair.Value)
                                             .ToDictionary(pair => pair.Key, pair => pair.Value);

                var fmotifsChain = new List<Fmotif>();
                foreach (var fmotif in sortedFmotifs.Keys)
                {
                    var newFmotif = new Fmotif(fmotif.FmotifType, 
                                              (PauseTreatment) musicSequence.PauseTreatment, 
                                               fmotif.Id);

                    var fmotifAlphabet = DbHelper.GetFmotifAlphabet(db, fmotif.Id);
                    var fmotifBuilding = DbHelper.GetFmotifBuilding(db, fmotif.Id);
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
                                                    new Duration(dbNote.Numerator,
                                                                 dbNote.Denominator,
                                                                 dbNote.Onumerator,
                                                                 dbNote.Odenominator, 1),
                                                    dbNote.Triplet,
                                                    dbNote.Tie,
                                                    (int)dbNote.Priority);
                        newNote.Id = dbNote.Id;
                        newFmotif.NoteList.Add(newNote);
                    }
                    fmotifsChain.Add(newFmotif);
                }
                var result = new Dictionary<string, object> { { "fmotifs", fmotifsChain },
                                                              { "sequentialTransfer", musicSequence.SequentialTransfer} };
                ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object> { { "data", result } });
                return View(musicSequence);
            }
        }
    }

}
