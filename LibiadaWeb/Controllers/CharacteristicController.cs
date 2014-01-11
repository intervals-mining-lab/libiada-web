using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{ 
    public class CharacteristicController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Characteristic/

        public ActionResult Index()
        {
            var characteristic = db.characteristic.Include("chain").Include("link").Include("characteristic_type");
            return View(characteristic.ToList());
        }

        //
        // GET: /Characteristic/Details/5

        public ActionResult Details(long id)
        {
            characteristic characteristic = db.characteristic.Single(c => c.id == id);
            if (characteristic == null)
            {
                return HttpNotFound();
            }
            return View(characteristic);
        }

        //
        // GET: /Characteristic/Create

        public ActionResult Create()
        {
            ViewBag.chain_id = new SelectList(db.chain, "id", "building");
            ViewBag.link_id = new SelectList(db.link, "id", "name");
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name");
            return View();
        } 

        //
        // POST: /Characteristic/Create

        [HttpPost]
        public ActionResult Create(characteristic characteristic)
        {
            if (ModelState.IsValid)
            {
                db.characteristic.AddObject(characteristic);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.chain_id = new SelectList(db.chain, "id", "building", characteristic.chain_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", characteristic.link_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
        }
        
        //
        // GET: /Characteristic/Edit/5
 
        public ActionResult Edit(long id)
        {
            characteristic characteristic = db.characteristic.Single(c => c.id == id);
            if (characteristic == null)
            {
                return HttpNotFound();
            }
            ViewBag.chain_id = new SelectList(db.chain, "id", "building", characteristic.chain_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", characteristic.link_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
        }

        //
        // POST: /Characteristic/Edit/5

        [HttpPost]
        public ActionResult Edit(characteristic characteristic)
        {
            if (ModelState.IsValid)
            {
                db.characteristic.Attach(characteristic);
                db.ObjectStateManager.ChangeObjectState(characteristic, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.chain_id = new SelectList(db.chain, "id", "building", characteristic.chain_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", characteristic.link_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
        }

        //
        // GET: /Characteristic/Delete/5
 
        public ActionResult Delete(long id)
        {
            characteristic characteristic = db.characteristic.Single(c => c.id == id);
            if (characteristic == null)
            {
                return HttpNotFound();
            }
            return View(characteristic);
        }

        //
        // POST: /Characteristic/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            characteristic characteristic = db.characteristic.Single(c => c.id == id);
            db.characteristic.DeleteObject(characteristic);
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