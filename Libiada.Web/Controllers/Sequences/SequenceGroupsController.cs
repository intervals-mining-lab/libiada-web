namespace Libiada.Web.Controllers.Sequences;

using Libiada.Web.Extensions;
using Libiada.Web.Helpers;
using Libiada.Web.Models.CalculatorsData;

using Newtonsoft.Json;

using EnumExtensions = Core.Extensions.EnumExtensions;

/// <summary>
/// The sequence groups controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequenceGroupsController : Controller
{
    /// <summary>
    /// The database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;
    private readonly IViewDataHelper viewDataHelper;

    public SequenceGroupsController(LibiadaDatabaseEntities db, IViewDataHelper viewDataHelper)
    {
        this.db = db;
        this.viewDataHelper = viewDataHelper;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    public async Task<ActionResult> Index()
    {
        var sequenceGroups = db.SequenceGroups.Include(s => s.Creator).Include(s => s.Modifier);
        return View(await sequenceGroups.ToListAsync());
    }

    /// <summary>
    /// The details.
    /// </summary>
    /// <param name="id">
    /// The id.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    public async Task<ActionResult> Details(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        SequenceGroup? sequenceGroup = await db.SequenceGroups
                                               .Include(sg => sg.Matters)
                                               .Include(sg => sg.Creator)
                                               .Include(sg => sg.Modifier)
                                               .SingleOrDefaultAsync(sg => sg.Id == id);

        if (sequenceGroup == null)
        {
            return NotFound();
        }

        return View(sequenceGroup);
    }

    /// <summary>
    /// The create.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Create()
    {
        var viewData = viewDataHelper.GetMattersData(1, int.MaxValue);
        viewData["sequenceGroupTypes"] = EnumExtensions.ToArray<SequenceGroupType>().ToSelectListWithNature();
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The create.
    /// </summary>
    /// <param name="sequenceGroup">
    /// The sequence group.
    /// </param>
    /// <param name="matterIds">
    /// The matters Ids.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(SequenceGroup sequenceGroup, long[] matterIds)
    {
        sequenceGroup.CreatorId = User.GetUserId();
        sequenceGroup.ModifierId = User.GetUserId();
        //ModelState.MarkFieldValid(nameof(sequenceGroup));

        if (ModelState.IsValid)
        {
            Matter[] matters = db.Matters.Where(m => matterIds.Contains(m.Id)).ToArray();
            foreach (Matter matter in matters)
            {
                sequenceGroup.Matters.Add(matter);
            }

            db.SequenceGroups.Add(sequenceGroup);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return View(sequenceGroup);
    }

    /// <summary>
    /// The edit.
    /// </summary>
    /// <param name="id">
    /// The id.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        SequenceGroup? sequenceGroup = await db.SequenceGroups.Include(m => m.Matters)
                                                              .SingleOrDefaultAsync(sg => sg.Id == id);
        if (sequenceGroup == null)
        {
            return NotFound();
        }

        var selectedMatterIds = sequenceGroup.Matters.Select(m => m.Id);
        var viewData = viewDataHelper.FillViewData(1, int.MaxValue, m => true, m => selectedMatterIds.Contains(m.Id), "Save");
        viewData["sequenceGroupTypes"] = EnumExtensions.ToArray<SequenceGroupType>().ToSelectListWithNature();

        // TODO: try to optimize this
        SelectListItemWithNature[] sequenceTypes = ((IEnumerable<SelectListItemWithNature>)viewData["sequenceTypes"]).ToArray();
        string sequenceTypeValue = Convert.ToByte(sequenceGroup.SequenceType).ToString();
        SelectListItemWithNature sequenceTypeSelectListItem = sequenceTypes.Single(st => st.Value == sequenceTypeValue);
        viewData["sequenceTypeIndex"] = Array.IndexOf(sequenceTypes, sequenceTypeSelectListItem);

        SelectListItemWithNature[] groups = ((IEnumerable<SelectListItemWithNature>)viewData["sequenceTypes"]).ToArray();
        string groupValue = Convert.ToByte(sequenceGroup.Group).ToString();
        SelectListItemWithNature groupSelectListItem = groups.Single(g => g.Value == groupValue);
        viewData["groupIndex"] = Array.IndexOf(groups, groupSelectListItem);

        ViewBag.data = JsonConvert.SerializeObject(viewData);
        
        return View(sequenceGroup);
    }

    /// <summary>
    /// The edit.
    /// </summary>
    /// <param name="sequenceGroup">
    /// The sequence group.
    /// </param>
    /// <param name="matterIds">
    /// The group matters ids.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(SequenceGroup sequenceGroup, long[] matterIds)
    {
        if (ModelState.IsValid)
        {
            SequenceGroup originalSequenceGroup = db.SequenceGroups.Include(sg => sg.Matters).Single(sg => sg.Id == sequenceGroup.Id);
            originalSequenceGroup.Name = sequenceGroup.Name;
            originalSequenceGroup.Nature = sequenceGroup.Nature;
            originalSequenceGroup.SequenceGroupType = sequenceGroup.SequenceGroupType;
            originalSequenceGroup.ModifierId = User.GetUserId();
            originalSequenceGroup.Group = sequenceGroup.Group;
            originalSequenceGroup.SequenceType = sequenceGroup.SequenceType;
            Matter[] matters = db.Matters.Where(m => matterIds.Contains(m.Id)).ToArray();
            originalSequenceGroup.Matters.Clear();
            foreach (var matter in matters)
            {
                originalSequenceGroup.Matters.Add(matter);
            }

            db.Entry(originalSequenceGroup).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return View(sequenceGroup);
    }

    /// <summary>
    /// The delete.
    /// </summary>
    /// <param name="id">
    /// The id.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        SequenceGroup? sequenceGroup = await db.SequenceGroups
                                               .Include(sg => sg.Matters)
                                               .Include(sg => sg.Creator)
                                               .Include(sg => sg.Modifier)
                                               .SingleOrDefaultAsync(sg => sg.Id == id);
        if (sequenceGroup == null)
        {
            return NotFound();
        }

        return View(sequenceGroup);
    }

    /// <summary>
    /// The delete confirmed.
    /// </summary>
    /// <param name="id">
    /// The id.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        SequenceGroup? sequenceGroup = await db.SequenceGroups.FindAsync(id);
        if (sequenceGroup == null)
        {
            return NotFound();
        }

        db.SequenceGroups.Remove(sequenceGroup);
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    /// <summary>
    /// The dispose.
    /// </summary>
    /// <param name="disposing">
    /// The disposing.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
