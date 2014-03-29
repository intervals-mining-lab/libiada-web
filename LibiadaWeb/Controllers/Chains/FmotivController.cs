using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    using System.Data.Entity;

    public class FmotivController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Fmotiv/

        public ActionResult Index()
        {
            var fmotiv = db.fmotiv.Include("matter").Include("notation").Include("piece_type").Include("fmotiv_type");
            return View(fmotiv.ToList());
        }

        //
        // GET: /Fmotiv/Details/5

        public ActionResult Details(long id)
        {
            fmotiv fmotiv = db.fmotiv.Single(f => f.id == id);
            if (fmotiv == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv);
        }

        //
        // GET: /Fmotiv/Create

        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.fmotiv_type_id = new SelectList(db.fmotiv_type, "id", "name");
            return View();
        } 

        //
        // POST: /Fmotiv/Create

        [HttpPost]
        public ActionResult Create(fmotiv fmotiv)
        {
            if (ModelState.IsValid)
            {
                db.fmotiv.AddObject(fmotiv);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", fmotiv.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", fmotiv.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", fmotiv.piece_type_id);
            ViewBag.fmotiv_type_id = new SelectList(db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            return View(fmotiv);
        }
        
        //
        // GET: /Fmotiv/Edit/5
 
        public ActionResult Edit(long id)
        {
            fmotiv fmotiv = db.fmotiv.Single(f => f.id == id);
            if (fmotiv == null)
            {
                return HttpNotFound();
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", fmotiv.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", fmotiv.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", fmotiv.piece_type_id);
            ViewBag.fmotiv_type_id = new SelectList(db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            return View(fmotiv);
        }

        //
        // POST: /Fmotiv/Edit/5

        [HttpPost]
        public ActionResult Edit(fmotiv fmotiv)
        {
            if (ModelState.IsValid)
            {
                db.fmotiv.Attach(fmotiv);
                db.ObjectStateManager.ChangeObjectState(fmotiv, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", fmotiv.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", fmotiv.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", fmotiv.piece_type_id);
            ViewBag.fmotiv_type_id = new SelectList(db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            return View(fmotiv);
        }

        //
        // GET: /Fmotiv/Delete/5
 
        public ActionResult Delete(long id)
        {
            fmotiv fmotiv = db.fmotiv.Single(f => f.id == id);
            if (fmotiv == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv);
        }

        //
        // POST: /Fmotiv/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            fmotiv fmotiv = db.fmotiv.Single(f => f.id == id);
            db.fmotiv.DeleteObject(fmotiv);
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