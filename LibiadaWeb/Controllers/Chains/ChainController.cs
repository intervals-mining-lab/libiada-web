using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    public class ChainController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Chain/
        public ActionResult Index()
        {
            var chain = db.chain.Include(c => c.matter).Include(c => c.notation).Include(c => c.piece_type).Include(c => c.remote_db);
            return View(chain.ToList());
        }

        // GET: /Chain/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            chain chain = db.chain.Find(id);
            if (chain == null)
            {
                return HttpNotFound();
            }
            return View(chain);
        }

        // GET: /Chain/Create
        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        // POST: /Chain/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,remote_db_id,modified,remote_id,description")] chain chain)
        {
            if (ModelState.IsValid)
            {
                db.chain.Add(chain);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", chain.remote_db_id);
            return View(chain);
        }

        // GET: /Chain/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            chain chain = db.chain.Find(id);
            if (chain == null)
            {
                return HttpNotFound();
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", chain.remote_db_id);
            return View(chain);
        }

        // POST: /Chain/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,remote_db_id,modified,remote_id,description")] chain chain)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chain).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", chain.remote_db_id);
            return View(chain);
        }

        // GET: /Chain/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            chain chain = db.chain.Find(id);
            if (chain == null)
            {
                return HttpNotFound();
            }
            return View(chain);
        }

        // POST: /Chain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            chain chain = db.chain.Find(id);
            db.chain.Remove(chain);
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
