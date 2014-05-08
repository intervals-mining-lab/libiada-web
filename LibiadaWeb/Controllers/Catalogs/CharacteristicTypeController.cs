using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class CharacteristicTypeController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /CharacteristicType/
        public ActionResult Index()
        {
            var characteristic_type = db.characteristic_type.Include(c => c.characteristic_group);
            return View(characteristic_type.ToList());
        }

        // GET: /CharacteristicType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic_type characteristic_type = db.characteristic_type.Find(id);
            if (characteristic_type == null)
            {
                return HttpNotFound();
            }
            return View(characteristic_type);
        }

        // GET: /CharacteristicType/Create
        public ActionResult Create()
        {
            ViewBag.characteristic_group_id = new SelectList(db.characteristic_group, "id", "name");
            return View();
        }

        // POST: /CharacteristicType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description,characteristic_group_id,class_name,linkable,full_chain_applicable,congeneric_chain_applicable,binary_chain_applicable")] characteristic_type characteristic_type)
        {
            if (ModelState.IsValid)
            {
                db.characteristic_type.Add(characteristic_type);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.characteristic_group_id = new SelectList(db.characteristic_group, "id", "name", characteristic_type.characteristic_group_id);
            return View(characteristic_type);
        }

        // GET: /CharacteristicType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic_type characteristic_type = db.characteristic_type.Find(id);
            if (characteristic_type == null)
            {
                return HttpNotFound();
            }
            ViewBag.characteristic_group_id = new SelectList(db.characteristic_group, "id", "name", characteristic_type.characteristic_group_id);
            return View(characteristic_type);
        }

        // POST: /CharacteristicType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description,characteristic_group_id,class_name,linkable,full_chain_applicable,congeneric_chain_applicable,binary_chain_applicable")] characteristic_type characteristic_type)
        {
            if (ModelState.IsValid)
            {
                db.Entry(characteristic_type).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.characteristic_group_id = new SelectList(db.characteristic_group, "id", "name", characteristic_type.characteristic_group_id);
            return View(characteristic_type);
        }

        // GET: /CharacteristicType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic_type characteristic_type = db.characteristic_type.Find(id);
            if (characteristic_type == null)
            {
                return HttpNotFound();
            }
            return View(characteristic_type);
        }

        // POST: /CharacteristicType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            characteristic_type characteristic_type = db.characteristic_type.Find(id);
            db.characteristic_type.Remove(characteristic_type);
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
