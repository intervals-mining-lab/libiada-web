using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{ 
    public class BuildingTypeController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /BuildingType/

        public ViewResult Index()
        {
            return View(db.building_type.ToList());
        }

        //
        // GET: /BuildingType/Details/5

        public ViewResult Details(int id)
        {
            building_type building_type = db.building_type.Single(b => b.id == id);
            return View(building_type);
        }

        //
        // GET: /BuildingType/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /BuildingType/Create

        [HttpPost]
        public ActionResult Create(building_type building_type)
        {
            if (ModelState.IsValid)
            {
                db.building_type.AddObject(building_type);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(building_type);
        }
        
        //
        // GET: /BuildingType/Edit/5
 
        public ActionResult Edit(int id)
        {
            building_type building_type = db.building_type.Single(b => b.id == id);
            return View(building_type);
        }

        //
        // POST: /BuildingType/Edit/5

        [HttpPost]
        public ActionResult Edit(building_type building_type)
        {
            if (ModelState.IsValid)
            {
                db.building_type.Attach(building_type);
                db.ObjectStateManager.ChangeObjectState(building_type, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(building_type);
        }

        //
        // GET: /BuildingType/Delete/5
 
        public ActionResult Delete(int id)
        {
            building_type building_type = db.building_type.Single(b => b.id == id);
            return View(building_type);
        }

        //
        // POST: /BuildingType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            building_type building_type = db.building_type.Single(b => b.id == id);
            db.building_type.DeleteObject(building_type);
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