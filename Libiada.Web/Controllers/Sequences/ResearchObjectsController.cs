namespace Libiada.Web.Controllers.Sequences;

using Libiada.Web.Extensions;
using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Libiada.Database.Tasks;
using Libiada.Database.Helpers;
using Libiada.Database.Models.Repositories.Sequences;

using Newtonsoft.Json;

using EnumExtensions = Core.Extensions.EnumExtensions;

/// <summary>
/// The research objects controller.
/// </summary>
[Authorize]
public class ResearchObjectsController : SequencesResearchObjectsController
{
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResearchObjectsController"/> class.
    /// </summary>
    public ResearchObjectsController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                             IViewDataHelper viewDataHelper,
                             ITaskManager taskManager,
                             INcbiHelper ncbiHelper,
                             IResearchObjectsCache cache)
        : base(TaskType.ResearchObjectImport, dbFactory, viewDataHelper, taskManager, ncbiHelper, cache)
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
        List<ResearchObject> researchObject = await db.ResearchObjects.Include(m => m.Multisequence).ToListAsync();

        if (!User.IsAdmin())
        {
            researchObject = researchObject.Where(m => m.Nature == Nature.Genetic).ToList();
        }

        return View(researchObject);

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
        ResearchObject? researchObject = db.ResearchObjects.Include(m => m.Multisequence).SingleOrDefault(m => m.Id == id);
        if (researchObject == null)
        {
            return NotFound();
        }

        ViewBag.SequencesCount = db.CombinedSequenceEntities.Count(c => c.ResearchObjectId == researchObject.Id);

        return View(researchObject);

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
        using var db = dbFactory.CreateDbContext();
        ResearchObject? researchObject = db.ResearchObjects.Include(m => m.Multisequence).SingleOrDefault(m => m.Id == id);
        if (researchObject == null)
        {
            return NotFound();
        }
        var data = new Dictionary<string, object>
            {
                { "natures", Extensions.EnumExtensions.GetSelectList(new[] { researchObject.Nature }) },
                { "groups", EnumExtensions.ToArray<Group>().ToSelectListWithNature() },
                { "sequenceTypes", EnumExtensions.ToArray<SequenceType>().ToSelectListWithNature() },
                { "sequencesCount", db.CombinedSequenceEntities.Count(c => c.ResearchObjectId == researchObject.Id) },
                { "researchObject", researchObject },
                { "multisequences", db.Multisequences.ToList() },
                { "nature", ((byte)researchObject.Nature).ToString() },
                { "group", ((byte)researchObject.Group).ToString() },
                { "sequenceType", ((byte)researchObject.SequenceType).ToString() }
            };

        ViewBag.data = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        return View(researchObject);

    }

    /// <summary>
    /// The edit.
    /// </summary>
    /// <param name="researchObject">
    /// The research object.
    /// </param>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    [HttpPost]
    public async Task<ActionResult> Edit(ResearchObject researchObject)
    {
        using var db = dbFactory.CreateDbContext();
        if (ModelState.IsValid)
        {
            db.Entry(researchObject).State = EntityState.Modified;
            await db.SaveChangesAsync();
            cache.Clear();
            return RedirectToAction("Index");
        }

        var data = new Dictionary<string, object>
            {
                { "natures", Extensions.EnumExtensions.GetSelectList(new[] { researchObject.Nature }) },
                { "groups", EnumExtensions.ToArray<Group>().ToSelectListWithNature() },
                { "sequenceTypes", EnumExtensions.ToArray<SequenceType>().ToSelectListWithNature() },
                { "sequencesCount", db.CombinedSequenceEntities.Count(c => c.ResearchObjectId == researchObject.Id) },
                { "researchObject", researchObject },
                { "multisequences", db.Multisequences.ToList() },
                { "nature", ((byte)researchObject.Nature).ToString() },
                { "group", ((byte)researchObject.Group).ToString() },
                { "sequenceType", ((byte)researchObject.SequenceType).ToString() }
            };

        ViewBag.data = JsonConvert.SerializeObject(data);
        return View(researchObject);

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
        ResearchObject? researchObject = await db.ResearchObjects.FindAsync(id);
        if (researchObject == null)
        {
            return NotFound();
        }

        ViewBag.SequencesCount = db.CombinedSequenceEntities.Count(c => c.ResearchObjectId == researchObject.Id);
        return View(researchObject);

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
        ResearchObject researchObject = await db.ResearchObjects.FindAsync(id);
        db.ResearchObjects.Remove(researchObject);
        await db.SaveChangesAsync();
        cache.Clear();
        return RedirectToAction("Index");
    }
}
