using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{ 
    public class CharacteristicGroupController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /CharacteristicGroup/

        public ViewResult Index()
        {
            return View(db.characteristic_group.ToList());
        }

        //
        // GET: /CharacteristicGroup/Details/5

        public ViewResult Details(int id)
        {
            characteristic_group characteristic_group = db.characteristic_group.Single(c => c.id == id);
            return View(characteristic_group);
        }

        //
        // GET: /CharacteristicGroup/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /CharacteristicGroup/Create

        [HttpPost]
        public ActionResult Create(characteristic_group characteristic_group)
        {
            if (ModelState.IsValid)
            {
                db.characteristic_group.AddObject(characteristic_group);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(characteristic_group);
        }
        
        //
        // GET: /CharacteristicGroup/Edit/5
 
        public ActionResult Edit(int id)
        {
            characteristic_group characteristic_group = db.characteristic_group.Single(c => c.id == id);
            return View(characteristic_group);
        }

        //
        // POST: /CharacteristicGroup/Edit/5

        [HttpPost]
        public ActionResult Edit(characteristic_group characteristic_group)
        {
            if (ModelState.IsValid)
            {
                db.characteristic_group.Attach(characteristic_group);
                db.ObjectStateManager.ChangeObjectState(characteristic_group, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(characteristic_group);
        }

        //
        // GET: /CharacteristicGroup/Delete/5
 
        public ActionResult Delete(int id)
        {
            characteristic_group characteristic_group = db.characteristic_group.Single(c => c.id == id);
            return View(characteristic_group);
        }

        //
        // POST: /CharacteristicGroup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            characteristic_group characteristic_group = db.characteristic_group.Single(c => c.id == id);
            db.characteristic_group.DeleteObject(characteristic_group);
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