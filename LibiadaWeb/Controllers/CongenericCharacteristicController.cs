using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{
    using System.Data.Entity;

    public class CongenericCharacteristicController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /CongenericCharacteristic/

        public ActionResult Index()
        {
            var congeneric_characteristic = db.congeneric_characteristic.Include("characteristic_type").Include("element").Include("link");
            return View(congeneric_characteristic.ToList());
        }

        //
        // GET: /CongenericCharacteristic/Details/5

        public ActionResult Details(long id)
        {
            congeneric_characteristic congeneric_characteristic = db.congeneric_characteristic.Single(h => h.id == id);
            if (congeneric_characteristic == null)
            {
                return HttpNotFound();
            }
            return View(congeneric_characteristic);
        }

        //
        // GET: /CongenericCharacteristic/Create

        public ActionResult Create()
        {
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name");
            ViewBag.element_id = new SelectList(db.element, "id", "value");
            ViewBag.link_id = new SelectList(db.link, "id", "name");
            return View();
        } 

        //
        // POST: /CongenericCharacteristic/Create

        [HttpPost]
        public ActionResult Create(congeneric_characteristic congeneric_characteristic)
        {
            if (ModelState.IsValid)
            {
                db.congeneric_characteristic.AddObject(congeneric_characteristic);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", congeneric_characteristic.element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", congeneric_characteristic.link_id);
            return View(congeneric_characteristic);
        }
        
        //
        // GET: /CongenericCharacteristic/Edit/5
 
        public ActionResult Edit(long id)
        {
            congeneric_characteristic congeneric_characteristic = db.congeneric_characteristic.Single(h => h.id == id);
            if (congeneric_characteristic == null)
            {
                return HttpNotFound();
            }
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", congeneric_characteristic.element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", congeneric_characteristic.link_id);
            return View(congeneric_characteristic);
        }

        //
        // POST: /CongenericCharacteristic/Edit/5

        [HttpPost]
        public ActionResult Edit(congeneric_characteristic congeneric_characteristic)
        {
            if (ModelState.IsValid)
            {
                db.congeneric_characteristic.Attach(congeneric_characteristic);
                db.ObjectStateManager.ChangeObjectState(congeneric_characteristic, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", congeneric_characteristic.element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", congeneric_characteristic.link_id);
            return View(congeneric_characteristic);
        }

        //
        // GET: /CongenericCharacteristic/Delete/5
 
        public ActionResult Delete(long id)
        {
            congeneric_characteristic congeneric_characteristic = db.congeneric_characteristic.Single(h => h.id == id);
            if (congeneric_characteristic == null)
            {
                return HttpNotFound();
            }
            return View(congeneric_characteristic);
        }

        //
        // POST: /CongenericCharacteristic/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            congeneric_characteristic congeneric_characteristic = db.congeneric_characteristic.Single(h => h.id == id);
            db.congeneric_characteristic.DeleteObject(congeneric_characteristic);
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