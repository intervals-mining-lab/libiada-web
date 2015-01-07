using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class AccidentalsController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: Accidentals
        public ActionResult Index()
        {
            return View(db.Accidental.ToList());
        }

        // GET: Accidentals/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Accidental accidental = db.Accidental.Find(id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        // GET: Accidentals/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Accidentals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description")] Accidental accidental)
        {
            if (ModelState.IsValid)
            {
                db.Accidental.Add(accidental);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(accidental);
        }

        // GET: Accidentals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Accidental accidental = db.Accidental.Find(id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        // POST: Accidentals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description")] Accidental accidental)
        {
            if (ModelState.IsValid)
            {
                db.Entry(accidental).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(accidental);
        }

        // GET: Accidentals/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Accidental accidental = db.Accidental.Find(id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        // POST: Accidentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Accidental accidental = db.Accidental.Find(id);
            db.Accidental.Remove(accidental);
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
