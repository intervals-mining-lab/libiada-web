namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Tasks;
using Libiada.Database.Helpers;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

/// <summary>
/// The combines sequences controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class CombinedSequencesEntityController : SequencesMattersController
{
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinedSequencesEntityController"/> class.
    /// </summary>
    public CombinedSequencesEntityController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                     IViewDataHelper viewDataHelper,
                                     ITaskManager taskManager,
                                     INcbiHelper ncbiHelper,
                                     Cache cache)
        : base(TaskType.SequencesUpload, dbFactory, viewDataHelper, taskManager, ncbiHelper, cache)
    {
        this.cache = cache;
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
        var sequences = db.CombinedSequenceEntities.Include(c => c.Matter);

        ViewBag.Sequences = await sequences.ToListAsync();

        return View();

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
            return BadRequest();
        }
        using var db = dbFactory.CreateDbContext();
        CombinedSequenceEntity? sequence = db.CombinedSequenceEntities.Include(c => c.Matter).Single(c => c.Id == id);
        if (sequence == null)
        {
            return NotFound();
        }

        return View(sequence);

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
            return BadRequest();
        }

        using var db = dbFactory.CreateDbContext();
        CombinedSequenceEntity? sequence = await db.CombinedSequenceEntities.FindAsync(id);
        if (sequence == null)
        {
            return NotFound();
        }
        RemoteDb[] remoteDb = sequence.RemoteDb == null ? [] : [(RemoteDb)sequence.RemoteDb];
        ViewBag.MatterId = new SelectList(cache.Matters.ToArray(), "Id", "Name", sequence.MatterId);
        ViewBag.Notation = Extensions.EnumExtensions.GetSelectList([sequence.Notation]);
        ViewBag.RemoteDb = Extensions.EnumExtensions.GetSelectList(remoteDb);
        return View(sequence);
    }

    /// <summary>
    /// The edit.
    /// </summary>
    /// <param name="sequence">
    /// The common sequence.
    /// </param>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    [HttpPost]
    public async Task<ActionResult> Edit(CombinedSequenceEntity sequence)
    {
        if (ModelState.IsValid)
        {
            using var db = dbFactory.CreateDbContext();
            db.Entry(sequence).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        RemoteDb[] remoteDb = sequence.RemoteDb == null ? [] : [(RemoteDb)sequence.RemoteDb];

        ViewBag.MatterId = new SelectList(cache.Matters.ToArray(), "Id", "Name", sequence.MatterId);
        ViewBag.Notation = Extensions.EnumExtensions.GetSelectList([sequence.Notation]);
        ViewBag.RemoteDb = Extensions.EnumExtensions.GetSelectList(remoteDb);
        return View(sequence);
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
            return BadRequest();
        }
        using var db = dbFactory.CreateDbContext();
        CombinedSequenceEntity sequence = db.CombinedSequenceEntities.Include(c => c.Matter).Single(c => c.Id == id);
        if (sequence == null)
        {
            return NotFound();
        }

        return View(sequence);
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
    public async Task<ActionResult> DeleteConfirmed(long id)
    {
        using var db = dbFactory.CreateDbContext();
        CombinedSequenceEntity sequence = await db.CombinedSequenceEntities.FindAsync(id);
        db.CombinedSequenceEntities.Remove(sequence);
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
