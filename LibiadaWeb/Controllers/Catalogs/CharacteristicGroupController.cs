using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class CharacteristicGroupController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /CharacteristicGroup/
        public ActionResult Index()
        {
            return View(db.characteristic_group.ToList());
        }

        // GET: /CharacteristicGroup/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic_group characteristic_group = db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return HttpNotFound();
            }
            return View(characteristic_group);
        }

        // GET: /CharacteristicGroup/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /CharacteristicGroup/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] characteristic_group characteristic_group)
        {
            if (ModelState.IsValid)
            {
                db.characteristic_group.Add(characteristic_group);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(characteristic_group);
        }

        // GET: /CharacteristicGroup/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic_group characteristic_group = db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return HttpNotFound();
            }
            return View(characteristic_group);
        }

        // POST: /CharacteristicGroup/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] characteristic_group characteristic_group)
        {
            if (ModelState.IsValid)
            {
                db.Entry(characteristic_group).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(characteristic_group);
        }

        // GET: /CharacteristicGroup/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic_group characteristic_group = db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return HttpNotFound();
            }
            return View(characteristic_group);
        }

        // POST: /CharacteristicGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            characteristic_group characteristic_group = db.characteristic_group.Find(id);
            db.characteristic_group.Remove(characteristic_group);
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
