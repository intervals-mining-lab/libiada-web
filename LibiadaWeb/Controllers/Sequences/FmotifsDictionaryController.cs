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

    /// <summary>
    /// The common sequences controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class FmotifsDictionaryController: SequencesMattersController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FmotifsDictionaryController"/> class.
        /// </summary>
        public FmotifsDictionaryController() : base(TaskType.CommonSequences)
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
        /// The id.
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

                var fmotifs = new Dictionary<Fmotif, (int, List<Note>)>();
                var notes = new List<List<Note>>();
                var musicChainAlphabet = DbHelper.GetMusicChainAlphabet(db, musicSequence.Id).Select(el => db.Fmotif.Single(f => f.Id == el)).ToList();
                var musicChainBuilding = DbHelper.GetMusicChainBuilding(db, musicSequence.Id);
                foreach(var fmotif in musicChainAlphabet)
                {
                    var fmotifAlphabet = DbHelper.GetFmotifAlphabet(db, fmotif.Id);
                    var fmotifBuilding = DbHelper.GetFmotifBuilding(db, fmotif.Id);
                    notes.Add(new List<Note>());
                    for (int i = 0; i < fmotifBuilding.Length; i++)
                    {
                        var index = fmotifAlphabet.ElementAt(fmotifBuilding[i] - 1);
                        notes.Last().Add(db.Note.Single(n => n.Id == index));
                    }
                }

                for (int i = 0; i < musicChainAlphabet.Count; i++)
                {
                    fmotifs.Add(musicChainAlphabet[i], (musicChainBuilding.Count(el => el == i + 1), notes[i]));
                }
                var data = fmotifs.OrderByDescending(pair => pair.Value.Item1).ToDictionary(pair => pair.Key.Id,
                    pair => (
                    Count:pair.Value.Item1,
                    Notes:pair.Value.Item2.Select(n => (n.Id, PitchsId : n.Pitch.Select(p => p.Midinumber).ToList(), n.Numerator, n.Denominator)).ToList()));
                ViewBag.Fmotifs = data;
                return View(musicSequence);
            }
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new LibiadaWebEntities())
            {
                MusicSequence musicSequence = await db.MusicSequence.FindAsync(id);
                if (musicSequence == null)
                {
                    return HttpNotFound();
                }

                ViewBag.MatterId = new SelectList(db.Matter.ToArray(), "Id", "Name", musicSequence.MatterId);
                ViewBag.Notation = EnumHelper.GetSelectList(typeof(Notation), musicSequence.Notation);
                ViewBag.RemoteDb = EnumHelper.GetSelectList(typeof(RemoteDb), musicSequence.RemoteDb);
                return View(musicSequence);
            }
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="musicSequence">
        /// The common sequence.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Notation,MatterId,RemoteDb,RemoteId,Description")] MusicSequence musicSequence)
        {
            using (var db = new LibiadaWebEntities())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(musicSequence).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                ViewBag.MatterId = new SelectList(db.Matter.ToArray(), "Id", "Name", musicSequence.MatterId);
                ViewBag.Notation = EnumHelper.GetSelectList(typeof(Notation), musicSequence.Notation);
                ViewBag.RemoteDb = EnumHelper.GetSelectList(typeof(RemoteDb), musicSequence.RemoteDb);
                return View(musicSequence);
            }
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public async Task<ActionResult> Delete(long? id)
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

                return View(musicSequence);
            }
        }

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            using (var db = new LibiadaWebEntities())
            {
                MusicSequence musicSequence = await db.MusicSequence.FindAsync(id);
                db.MusicSequence.Remove(musicSequence);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }
    }
}
