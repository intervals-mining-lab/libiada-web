using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{ 
    public class MusicChainController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /MusicChain/

        public ActionResult Index()
        {
            var music_chain = db.music_chain.Include("instrument").Include("matter").Include("notation").Include("piece_type");
            return View(music_chain.ToList());
        }

        //
        // GET: /MusicChain/Details/5

        public ActionResult Details(long id)
        {
            music_chain music_chain = db.music_chain.Single(m => m.id == id);
            if (music_chain == null)
            {
                return HttpNotFound();
            }
            return View(music_chain);
        }

        //
        // GET: /MusicChain/Create

        public ActionResult Create()
        {
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name");
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            return View();
        } 

        //
        // POST: /MusicChain/Create

        [HttpPost]
        public ActionResult Create(music_chain music_chain)
        {
            if (ModelState.IsValid)
            {
                db.music_chain.AddObject(music_chain);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name", music_chain.instrument_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            return View(music_chain);
        }
        
        //
        // GET: /MusicChain/Edit/5
 
        public ActionResult Edit(long id)
        {
            music_chain music_chain = db.music_chain.Single(m => m.id == id);
            if (music_chain == null)
            {
                return HttpNotFound();
            }
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name", music_chain.instrument_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            return View(music_chain);
        }

        //
        // POST: /MusicChain/Edit/5

        [HttpPost]
        public ActionResult Edit(music_chain music_chain)
        {
            if (ModelState.IsValid)
            {
                db.music_chain.Attach(music_chain);
                db.ObjectStateManager.ChangeObjectState(music_chain, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name", music_chain.instrument_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            return View(music_chain);
        }

        //
        // GET: /MusicChain/Delete/5
 
        public ActionResult Delete(long id)
        {
            music_chain music_chain = db.music_chain.Single(m => m.id == id);
            if (music_chain == null)
            {
                return HttpNotFound();
            }
            return View(music_chain);
        }

        //
        // POST: /MusicChain/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            music_chain music_chain = db.music_chain.Single(m => m.id == id);
            db.music_chain.DeleteObject(music_chain);
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