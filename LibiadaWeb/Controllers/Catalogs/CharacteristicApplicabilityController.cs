using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class CharacteristicApplicabilityController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /CharacteristicApplicability/

        public ActionResult Index()
        {
            return View(db.characteristic_applicability.ToList());
        }

        //
        // GET: /CharacteristicApplicability/Details/5

        public ActionResult Details(int id = 0)
        {
            characteristic_applicability characteristic_applicability = db.characteristic_applicability.Single(c => c.id == id);
            if (characteristic_applicability == null)
            {
                return HttpNotFound();
            }
            return View(characteristic_applicability);
        }

        //
        // GET: /CharacteristicApplicability/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /CharacteristicApplicability/Create

        [HttpPost]
        public ActionResult Create(characteristic_applicability characteristic_applicability)
        {
            if (ModelState.IsValid)
            {
                db.characteristic_applicability.AddObject(characteristic_applicability);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(characteristic_applicability);
        }

        //
        // GET: /CharacteristicApplicability/Edit/5

        public ActionResult Edit(int id = 0)
        {
            characteristic_applicability characteristic_applicability = db.characteristic_applicability.Single(c => c.id == id);
            if (characteristic_applicability == null)
            {
                return HttpNotFound();
            }
            return View(characteristic_applicability);
        }

        //
        // POST: /CharacteristicApplicability/Edit/5

        [HttpPost]
        public ActionResult Edit(characteristic_applicability characteristic_applicability)
        {
            if (ModelState.IsValid)
            {
                db.characteristic_applicability.Attach(characteristic_applicability);
                db.ObjectStateManager.ChangeObjectState(characteristic_applicability, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(characteristic_applicability);
        }

        //
        // GET: /CharacteristicApplicability/Delete/5

        public ActionResult Delete(int id = 0)
        {
            characteristic_applicability characteristic_applicability = db.characteristic_applicability.Single(c => c.id == id);
            if (characteristic_applicability == null)
            {
                return HttpNotFound();
            }
            return View(characteristic_applicability);
        }

        //
        // POST: /CharacteristicApplicability/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            characteristic_applicability characteristic_applicability = db.characteristic_applicability.Single(c => c.id == id);
            db.characteristic_applicability.DeleteObject(characteristic_applicability);
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