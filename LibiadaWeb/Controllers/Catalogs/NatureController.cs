using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class NatureController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Nature/
        public ActionResult Index()
        {
            return View(db.nature.ToList());
        }

        // GET: /Nature/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            nature nature = db.nature.Find(id);
            if (nature == null)
            {
                return HttpNotFound();
            }
            return View(nature);
        }

        // GET: /Nature/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Nature/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] nature nature)
        {
            if (ModelState.IsValid)
            {
                db.nature.Add(nature);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(nature);
        }

        // GET: /Nature/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            nature nature = db.nature.Find(id);
            if (nature == null)
            {
                return HttpNotFound();
            }
            return View(nature);
        }

        // POST: /Nature/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] nature nature)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nature).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(nature);
        }

        // GET: /Nature/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            nature nature = db.nature.Find(id);
            if (nature == null)
            {
                return HttpNotFound();
            }
            return View(nature);
        }

        // POST: /Nature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            nature nature = db.nature.Find(id);
            db.nature.Remove(nature);
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
