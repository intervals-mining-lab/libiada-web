namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Tasks;
using Libiada.Database.Helpers;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Libiada.Core.Extensions;

/// <summary>
/// The common sequences controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class CommonSequencesController : SequencesMattersController
{
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSequencesController"/> class.
    /// </summary>
    public CommonSequencesController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                     IViewDataHelper viewDataHelper,
                                     ITaskManager taskManager,
                                     INcbiHelper ncbiHelper,
                                     Cache cache)
        : base(TaskType.CommonSequences, dbFactory, viewDataHelper, taskManager, ncbiHelper, cache)
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
        var commonSequence = db.CommonSequences.Include(c => c.Matter).Select(cs => new SequenceViewModel()
        {
            Id = cs.Id,
            Created = cs.Created.ToString(),
            Description = cs.Description ?? "",
            MatterName = cs.Matter.Name,
            Modified = cs.Modified.ToString(),
            Notation = cs.Notation.GetDisplayValue(),
            RemoteDb = cs.RemoteDb.ToString() ?? "",
            RemoteId = cs.RemoteId ?? ""

        });

        ViewBag.Sequences = await commonSequence.ToListAsync();

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
        CommonSequence commonSequence = db.CommonSequences.Include(c => c.Matter).Single(c => c.Id == id);
        if (commonSequence == null)
        {
            return NotFound();
        }

        return View(commonSequence);

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
        CommonSequence? commonSequence = await db.CommonSequences.FindAsync(id);
        if (commonSequence == null)
        {
            return NotFound();
        }
        RemoteDb[] remoteDb = commonSequence.RemoteDb == null ? [] : [(RemoteDb)commonSequence.RemoteDb];
        ViewBag.MatterId = new SelectList(cache.Matters.ToArray(), "Id", "Name", commonSequence.MatterId);
        ViewBag.Notation = Extensions.EnumExtensions.GetSelectList(new[] { commonSequence.Notation });
        ViewBag.RemoteDb = Extensions.EnumExtensions.GetSelectList(remoteDb);
        return View(commonSequence);
    }

    /// <summary>
    /// The edit.
    /// </summary>
    /// <param name="commonSequence">
    /// The common sequence.
    /// </param>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(CommonSequence commonSequence)
    {
        if (ModelState.IsValid)
        {
            using var db = dbFactory.CreateDbContext();
            db.Entry(commonSequence).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        RemoteDb[] remoteDb = commonSequence.RemoteDb == null ? [] : [(RemoteDb)commonSequence.RemoteDb];

        ViewBag.MatterId = new SelectList(cache.Matters.ToArray(), "Id", "Name", commonSequence.MatterId);
        ViewBag.Notation = Extensions.EnumExtensions.GetSelectList(new[] { commonSequence.Notation });
        ViewBag.RemoteDb = Extensions.EnumExtensions.GetSelectList(remoteDb);
        return View(commonSequence);
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
        CommonSequence commonSequence = db.CommonSequences.Include(c => c.Matter).Single(c => c.Id == id);
        if (commonSequence == null)
        {
            return NotFound();
        }

        return View(commonSequence);
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
        using var db = dbFactory.CreateDbContext();
        CommonSequence commonSequence = await db.CommonSequences.FindAsync(id);
        db.CommonSequences.Remove(commonSequence);
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    internal readonly struct SequenceViewModel()
    {
        public readonly long Id { get; init; }
        public readonly string MatterName { get; init; }
        public readonly string Notation { get; init; }
        public readonly string RemoteDb { get; init; }
        public readonly string RemoteId { get; init; }
        public readonly string Description { get; init; }
        public readonly string Created { get; init; }
        public readonly string Modified { get; init; }
    }
}
