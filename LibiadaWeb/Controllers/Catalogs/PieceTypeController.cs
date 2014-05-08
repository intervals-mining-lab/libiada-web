using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class PieceTypeController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /PieceType/
        public ActionResult Index()
        {
            var piece_type = db.piece_type.Include(p => p.nature);
            return View(piece_type.ToList());
        }

        // GET: /PieceType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            piece_type piece_type = db.piece_type.Find(id);
            if (piece_type == null)
            {
                return HttpNotFound();
            }
            return View(piece_type);
        }

        // GET: /PieceType/Create
        public ActionResult Create()
        {
            ViewBag.nature_id = new SelectList(db.nature, "id", "name");
            return View();
        }

        // POST: /PieceType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description,nature_id")] piece_type piece_type)
        {
            if (ModelState.IsValid)
            {
                db.piece_type.Add(piece_type);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", piece_type.nature_id);
            return View(piece_type);
        }

        // GET: /PieceType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            piece_type piece_type = db.piece_type.Find(id);
            if (piece_type == null)
            {
                return HttpNotFound();
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", piece_type.nature_id);
            return View(piece_type);
        }

        // POST: /PieceType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description,nature_id")] piece_type piece_type)
        {
            if (ModelState.IsValid)
            {
                db.Entry(piece_type).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", piece_type.nature_id);
            return View(piece_type);
        }

        // GET: /PieceType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            piece_type piece_type = db.piece_type.Find(id);
            if (piece_type == null)
            {
                return HttpNotFound();
            }
            return View(piece_type);
        }

        // POST: /PieceType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            piece_type piece_type = db.piece_type.Find(id);
            db.piece_type.Remove(piece_type);
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
