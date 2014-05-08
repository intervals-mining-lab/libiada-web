using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class TranslatorController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Translator/
        public ActionResult Index()
        {
            return View(db.translator.ToList());
        }

        // GET: /Translator/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            translator translator = db.translator.Find(id);
            if (translator == null)
            {
                return HttpNotFound();
            }
            return View(translator);
        }

        // GET: /Translator/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Translator/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] translator translator)
        {
            if (ModelState.IsValid)
            {
                db.translator.Add(translator);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(translator);
        }

        // GET: /Translator/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            translator translator = db.translator.Find(id);
            if (translator == null)
            {
                return HttpNotFound();
            }
            return View(translator);
        }

        // POST: /Translator/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] translator translator)
        {
            if (ModelState.IsValid)
            {
                db.Entry(translator).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(translator);
        }

        // GET: /Translator/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            translator translator = db.translator.Find(id);
            if (translator == null)
            {
                return HttpNotFound();
            }
            return View(translator);
        }

        // POST: /Translator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            translator translator = db.translator.Find(id);
            db.translator.Remove(translator);
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
