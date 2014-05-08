using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class FmotivTypeController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /FmotivType/
        public ActionResult Index()
        {
            return View(db.fmotiv_type.ToList());
        }

        // GET: /FmotivType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            fmotiv_type fmotiv_type = db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv_type);
        }

        // GET: /FmotivType/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /FmotivType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] fmotiv_type fmotiv_type)
        {
            if (ModelState.IsValid)
            {
                db.fmotiv_type.Add(fmotiv_type);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fmotiv_type);
        }

        // GET: /FmotivType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            fmotiv_type fmotiv_type = db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv_type);
        }

        // POST: /FmotivType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] fmotiv_type fmotiv_type)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fmotiv_type).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fmotiv_type);
        }

        // GET: /FmotivType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            fmotiv_type fmotiv_type = db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv_type);
        }

        // POST: /FmotivType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            fmotiv_type fmotiv_type = db.fmotiv_type.Find(id);
            db.fmotiv_type.Remove(fmotiv_type);
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
