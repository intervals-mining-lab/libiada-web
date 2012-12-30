using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{ 
    public class BinaryCharacteristicController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /BinaryCharacteristic/

        public ActionResult Index()
        {
            var binary_characteristic = db.binary_characteristic.Include("chain").Include("element").Include("link_up").Include("element1").Include("characteristic_type");
            return View(binary_characteristic.ToList());
        }

        //
        // GET: /BinaryCharacteristic/Details/5

        public ActionResult Details(long id)
        {
            binary_characteristic binary_characteristic = db.binary_characteristic.Single(b => b.id == id);
            if (binary_characteristic == null)
            {
                return HttpNotFound();
            }
            return View(binary_characteristic);
        }

        //
        // GET: /BinaryCharacteristic/Create

        public ActionResult Create()
        {
            ViewBag.chain_id = new SelectList(db.chain, "id", "building");
            ViewBag.first_element_id = new SelectList(db.element, "id", "value");
            ViewBag.link_up_id = new SelectList(db.link_up, "id", "name");
            ViewBag.second_element_id = new SelectList(db.element, "id", "value");
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name");
            return View();
        } 

        //
        // POST: /BinaryCharacteristic/Create

        [HttpPost]
        public ActionResult Create(binary_characteristic binary_characteristic)
        {
            if (ModelState.IsValid)
            {
                db.binary_characteristic.AddObject(binary_characteristic);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.chain_id = new SelectList(db.chain, "id", "building", binary_characteristic.chain_id);
            ViewBag.first_element_id = new SelectList(db.element, "id", "value", binary_characteristic.first_element_id);
            ViewBag.link_up_id = new SelectList(db.link_up, "id", "name", binary_characteristic.link_up_id);
            ViewBag.second_element_id = new SelectList(db.element, "id", "value", binary_characteristic.second_element_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
        }
        
        //
        // GET: /BinaryCharacteristic/Edit/5
 
        public ActionResult Edit(long id)
        {
            binary_characteristic binary_characteristic = db.binary_characteristic.Single(b => b.id == id);
            if (binary_characteristic == null)
            {
                return HttpNotFound();
            }
            ViewBag.chain_id = new SelectList(db.chain, "id", "building", binary_characteristic.chain_id);
            ViewBag.first_element_id = new SelectList(db.element, "id", "value", binary_characteristic.first_element_id);
            ViewBag.link_up_id = new SelectList(db.link_up, "id", "name", binary_characteristic.link_up_id);
            ViewBag.second_element_id = new SelectList(db.element, "id", "value", binary_characteristic.second_element_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
        }

        //
        // POST: /BinaryCharacteristic/Edit/5

        [HttpPost]
        public ActionResult Edit(binary_characteristic binary_characteristic)
        {
            if (ModelState.IsValid)
            {
                db.binary_characteristic.Attach(binary_characteristic);
                db.ObjectStateManager.ChangeObjectState(binary_characteristic, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.chain_id = new SelectList(db.chain, "id", "building", binary_characteristic.chain_id);
            ViewBag.first_element_id = new SelectList(db.element, "id", "value", binary_characteristic.first_element_id);
            ViewBag.link_up_id = new SelectList(db.link_up, "id", "name", binary_characteristic.link_up_id);
            ViewBag.second_element_id = new SelectList(db.element, "id", "value", binary_characteristic.second_element_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
        }

        //
        // GET: /BinaryCharacteristic/Delete/5
 
        public ActionResult Delete(long id)
        {
            binary_characteristic binary_characteristic = db.binary_characteristic.Single(b => b.id == id);
            if (binary_characteristic == null)
            {
                return HttpNotFound();
            }
            return View(binary_characteristic);
        }

        //
        // POST: /BinaryCharacteristic/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            binary_characteristic binary_characteristic = db.binary_characteristic.Single(b => b.id == id);
            db.binary_characteristic.DeleteObject(binary_characteristic);
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