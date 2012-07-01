using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaWeb;

namespace LibiadaWeb.Controllers
{ 
    public class MatterController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Matter/

        public ViewResult Index()
        {
            var matter = db.matter.Include("nature").Include("remote_db");
            return View(matter.ToList());
        }

        //
        // GET: /Matter/Details/5

        public ViewResult Details(long id)
        {
            matter matter = db.matter.Single(m => m.id == id);
            return View(matter);
        }

        //
        // GET: /Matter/Create

        public ActionResult Create()
        {
            ViewBag.nature_id = new SelectList(db.nature, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            return View();
        } 

        //
        // POST: /Matter/Create

        [HttpPost]
        public ActionResult Create(matter matter, String chaintext, int notationId)
        {
            
            if (ModelState.IsValid)
            {
                Chain libiadaChain = new Chain(chaintext);
                chain result = new chain();

                String stringBuilding = "";
                for (int j = 0; j < libiadaChain.Building.Length; j++)
                {
                    stringBuilding += libiadaChain.Building[j] + "|";
                }
                stringBuilding = stringBuilding.Substring(0, stringBuilding.Length - 1);
                result.building = stringBuilding;
                result.dissimilar = false;
                result.building_type_id = 1;
                result.notation_id = notationId;
                result.matter = matter;
                result.creation_date = new DateTimeOffset(DateTime.Now);

                for (int i = 0; i < libiadaChain.Alphabet.power; i++)
                {
                    alphabet alphabetElement = new alphabet();
                    alphabetElement.chain = result;
                    alphabetElement.number = i + 1;
                    String strElem = libiadaChain.Alphabet[i].ToString();
                    alphabetElement.element = db.element.Single(e => e.notation_id == notationId && e.value.Equals(strElem));
                    db.alphabet.AddObject(alphabetElement);
                }

                db.chain.AddObject(result);
                db.matter.AddObject(matter);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", matter.remote_db_id);
            return View(matter);
        }
        
        //
        // GET: /Matter/Edit/5
 
        public ActionResult Edit(long id)
        {
            matter matter = db.matter.Single(m => m.id == id);
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", matter.remote_db_id);
            return View(matter);
        }

        //
        // POST: /Matter/Edit/5

        [HttpPost]
        public ActionResult Edit(matter matter)
        {
            if (ModelState.IsValid)
            {
                db.matter.Attach(matter);
                db.ObjectStateManager.ChangeObjectState(matter, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", matter.remote_db_id);
            return View(matter);
        }

        //
        // GET: /Matter/Delete/5
 
        public ActionResult Delete(long id)
        {
            matter matter = db.matter.Single(m => m.id == id);
            return View(matter);
        }

        //
        // POST: /Matter/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            matter matter = db.matter.Single(m => m.id == id);
            db.matter.DeleteObject(matter);
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