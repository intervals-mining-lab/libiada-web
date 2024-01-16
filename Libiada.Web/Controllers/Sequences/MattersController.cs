namespace Libiada.Web.Controllers.Sequences;

using Libiada.Web.Extensions;
using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Libiada.Database.Tasks;
using Libiada.Database.Helpers;

using Newtonsoft.Json;

using EnumExtensions = Libiada.Core.Extensions.EnumExtensions;

/// <summary>
/// The matters controller.
/// </summary>
[Authorize]
public class MattersController : SequencesMattersController
{
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MattersController"/> class.
    /// </summary>
    public MattersController(ILibiadaDatabaseEntitiesFactory dbFactory, 
                             IViewDataHelper viewDataHelper, 
                             ITaskManager taskManager,
                             INcbiHelper ncbiHelper,
                             Cache cache)
        : base(TaskType.Matters, dbFactory, viewDataHelper, taskManager, ncbiHelper, cache)
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
        List<Matter> matter = await dbFactory.CreateDbContext().Matters.Include(m => m.Multisequence).ToListAsync();

        if (!User.IsAdmin())
        {
            matter = matter.Where(m => m.Nature == Nature.Genetic).ToList();
        }

        return View(matter);

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
    public ActionResult Details(long? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        using var db = dbFactory.CreateDbContext();
        Matter? matter = db.Matters.Include(m => m.Multisequence).SingleOrDefault(m => m.Id == id);
        if (matter == null)
        {
            return NotFound();
        }

        ViewBag.SequencesCount = db.CommonSequences.Count(c => c.MatterId == matter.Id);

        return View(matter);

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
    public ActionResult Edit(long? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        var db = dbFactory.CreateDbContext();
        Matter? matter = db.Matters.Include(m => m.Multisequence).SingleOrDefault(m => m.Id == id);
        if (matter == null)
        {
            return NotFound();
        }
        var data = new Dictionary<string, object>
            {
                { "natures", Extensions.EnumExtensions.GetSelectList(new[] { matter.Nature }) },
                { "groups", EnumExtensions.ToArray<Group>().ToSelectListWithNature() },
                { "sequenceTypes", EnumExtensions.ToArray<SequenceType>().ToSelectListWithNature() },
                { "sequencesCount", db.CommonSequences.Count(c => c.MatterId == matter.Id) },
                { "matter", matter },
                { "multisequences", db.Multisequences.ToList() },
                { "nature", ((byte)matter.Nature).ToString() },
                { "group", ((byte)matter.Group).ToString() },
                { "sequenceType", ((byte)matter.SequenceType).ToString() }
            };

        ViewBag.data = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        return View(matter);

    }

    /// <summary>
    /// The edit.
    /// </summary>
    /// <param name="matter">
    /// The matter.
    /// </param>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(
    // [Bind(Include = "Id,Name,Nature,Description,Group,SequenceType,MultisequenceId,MultisequenceNumber,CollectionCountry,CollectionDate")] 
    Matter matter)
    {
        using var db = dbFactory.CreateDbContext();
        if (ModelState.IsValid)
        {
            db.Entry(matter).State = EntityState.Modified;
            await db.SaveChangesAsync();
            cache.Clear();
            return RedirectToAction("Index");
        }

        var data = new Dictionary<string, object>
            {
                { "natures", Extensions.EnumExtensions.GetSelectList(new[] { matter.Nature }) },
                { "groups", EnumExtensions.ToArray<Group>().ToSelectListWithNature() },
                { "sequenceTypes", EnumExtensions.ToArray<SequenceType>().ToSelectListWithNature() },
                { "sequencesCount", db.CommonSequences.Count(c => c.MatterId == matter.Id) },
                { "matter", matter },
                { "multisequences", db.Multisequences.ToList() },
                { "nature", ((byte)matter.Nature).ToString() },
                { "group", ((byte)matter.Group).ToString() },
                { "sequenceType", ((byte)matter.SequenceType).ToString() }
            };

        ViewBag.data = JsonConvert.SerializeObject(data);
        return View(matter);

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
        Matter? matter = await db.Matters.FindAsync(id);
        if (matter == null)
        {
            return NotFound();
        }

        ViewBag.SequencesCount = db.CommonSequences.Count(c => c.MatterId == matter.Id);
        return View(matter);

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
        var db = dbFactory.CreateDbContext();
        Matter matter = await db.Matters.FindAsync(id);
        db.Matters.Remove(matter);
        await db.SaveChangesAsync();
        cache.Clear();
        return RedirectToAction("Index");
    }
}
