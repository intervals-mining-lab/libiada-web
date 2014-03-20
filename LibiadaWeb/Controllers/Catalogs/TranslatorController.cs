using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    public class TranslatorController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Translator/

        public ActionResult Index()
        {
            return View(db.translator.ToList());
        }

        //
        // GET: /Translator/Details/5

        public ActionResult Details(int id = 0)
        {
            translator translator = db.translator.Single(t => t.id == id);
            if (translator == null)
            {
                return HttpNotFound();
            }
            return View(translator);
        }

        //
        // GET: /Translator/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Translator/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(translator translator)
        {
            if (ModelState.IsValid)
            {
                db.translator.AddObject(translator);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(translator);
        }

        //
        // GET: /Translator/Edit/5

        public ActionResult Edit(int id = 0)
        {
            translator translator = db.translator.Single(t => t.id == id);
            if (translator == null)
            {
                return HttpNotFound();
            }
            return View(translator);
        }

        //
        // POST: /Translator/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(translator translator)
        {
            if (ModelState.IsValid)
            {
                db.translator.Attach(translator);
                db.ObjectStateManager.ChangeObjectState(translator, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(translator);
        }

        //
        // GET: /Translator/Delete/5

        public ActionResult Delete(int id = 0)
        {
            translator translator = db.translator.Single(t => t.id == id);
            if (translator == null)
            {
                return HttpNotFound();
            }
            return View(translator);
        }

        //
        // POST: /Translator/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            translator translator = db.translator.Single(t => t.id == id);
            db.translator.DeleteObject(translator);
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