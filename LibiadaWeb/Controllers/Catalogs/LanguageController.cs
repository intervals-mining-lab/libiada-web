using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;

    public class LanguageController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Language/

        public ActionResult Index()
        {
            return View(db.language.ToList());
        }

        //
        // GET: /Language/Details/5

        public ActionResult Details(int id)
        {
            language language = db.language.Single(l => l.id == id);
            if (language == null)
            {
                return HttpNotFound();
            }
            return View(language);
        }

        //
        // GET: /Language/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Language/Create

        [HttpPost]
        public ActionResult Create(language language)
        {
            if (ModelState.IsValid)
            {
                db.language.AddObject(language);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(language);
        }
        
        //
        // GET: /Language/Edit/5
 
        public ActionResult Edit(int id)
        {
            language language = db.language.Single(l => l.id == id);
            if (language == null)
            {
                return HttpNotFound();
            }
            return View(language);
        }

        //
        // POST: /Language/Edit/5

        [HttpPost]
        public ActionResult Edit(language language)
        {
            if (ModelState.IsValid)
            {
                db.language.Attach(language);
                db.ObjectStateManager.ChangeObjectState(language, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(language);
        }

        //
        // GET: /Language/Delete/5
 
        public ActionResult Delete(int id)
        {
            language language = db.language.Single(l => l.id == id);
            if (language == null)
            {
                return HttpNotFound();
            }
            return View(language);
        }

        //
        // POST: /Language/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            language language = db.language.Single(l => l.id == id);
            db.language.DeleteObject(language);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}