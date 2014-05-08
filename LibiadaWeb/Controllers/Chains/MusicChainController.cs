using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    public class MusicChainController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /MusicChain/
        public ActionResult Index()
        {
            var music_chain = db.music_chain.Include(m => m.matter).Include(m => m.notation).Include(m => m.piece_type).Include(m => m.remote_db);
            return View(music_chain.ToList());
        }

        // GET: /MusicChain/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            music_chain music_chain = db.music_chain.Find(id);
            if (music_chain == null)
            {
                return HttpNotFound();
            }
            return View(music_chain);
        }

        // GET: /MusicChain/Create
        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        // POST: /MusicChain/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,remote_db_id,remote_id,modified,description")] music_chain music_chain)
        {
            if (ModelState.IsValid)
            {
                db.music_chain.Add(music_chain);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", music_chain.remote_db_id);
            return View(music_chain);
        }

        // GET: /MusicChain/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            music_chain music_chain = db.music_chain.Find(id);
            if (music_chain == null)
            {
                return HttpNotFound();
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", music_chain.remote_db_id);
            return View(music_chain);
        }

        // POST: /MusicChain/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,remote_db_id,remote_id,modified,description")] music_chain music_chain)
        {
            if (ModelState.IsValid)
            {
                db.Entry(music_chain).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", music_chain.remote_db_id);
            return View(music_chain);
        }

        // GET: /MusicChain/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            music_chain music_chain = db.music_chain.Find(id);
            if (music_chain == null)
            {
                return HttpNotFound();
            }
            return View(music_chain);
        }

        // POST: /MusicChain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            music_chain music_chain = db.music_chain.Find(id);
            db.music_chain.Remove(music_chain);
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
