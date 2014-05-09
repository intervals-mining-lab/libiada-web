using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    using LibiadaWeb.Helpers;

    public class MatterController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Matter/
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            var matter = db.matter.Include(m => m.nature);
            return View(matter.ToList());
        }

        // GET: /Matter/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            matter matter = db.matter.Find(id);
            if (matter == null)
            {
                return HttpNotFound();
            }
            return View(matter);
        }

        // GET: /Matter/Create
        public ActionResult Create()
        {
            ViewBag.nature_id = new SelectList(db.nature, "id", "name");
            return View();
        }

        // POST: /Matter/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,nature_id,description,created,modified")] matter matter)
        {
            if (ModelState.IsValid)
            {
                db.matter.Add(matter);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        // GET: /Matter/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            matter matter = db.matter.Find(id);
            if (matter == null)
            {
                return HttpNotFound();
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        // POST: /Matter/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,nature_id,description,created,modified")] matter matter)
        {
            if (ModelState.IsValid)
            {
                db.Entry(matter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        // GET: /Matter/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            matter matter = db.matter.Find(id);
            if (matter == null)
            {
                return HttpNotFound();
            }
            return View(matter);
        }

        // POST: /Matter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            matter matter = db.matter.Find(id);
            db.matter.Remove(matter);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
