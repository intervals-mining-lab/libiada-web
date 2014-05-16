using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    public class ElementController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Element/
        public ActionResult Index()
        {
            var element = db.element.Include(e => e.notation);
            return View(element.ToList());
        }

        // GET: /Element/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            element element = db.element.Find(id);
            if (element == null)
            {
                return HttpNotFound();
            }
            return View(element);
        }

        // GET: /Element/Create
        public ActionResult Create()
        {
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            return View();
        }

        // POST: /Element/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,value,description,name,notation_id")] element element)
        {
            if (ModelState.IsValid)
            {
                db.element.Add(element);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.notation_id = new SelectList(db.notation, "id", "name", element.notation_id);
            return View(element);
        }

        // GET: /Element/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            element element = db.element.Find(id);
            if (element == null)
            {
                return HttpNotFound();
            }
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", element.notation_id);
            return View(element);
        }

        // POST: /Element/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,value,description,name,notation_id,created,modified")] element element)
        {
            if (ModelState.IsValid)
            {
                db.Entry(element).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", element.notation_id);
            return View(element);
        }

        // GET: /Element/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            element element = db.element.Find(id);
            if (element == null)
            {
                return HttpNotFound();
            }
            return View(element);
        }

        // POST: /Element/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            element element = db.element.Find(id);
            db.element.Remove(element);
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
