using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaWeb;
using DownLoadedFile = System.IO.File;

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
        public ActionResult Create(matter matter, int notationId)
        {

            if (ModelState.IsValid)
            {
                string stringChain = "";
                var MyFileCollection = Request.Files[0];
                var MyFile = MyFileCollection;

                var FileLen = MyFile.ContentLength;
                byte[] input = new byte[FileLen];

                // Initialize the stream.
                var fileStream = MyFile.InputStream;

                // Read the file into the byte array.
                fileStream.Read(input, 0, FileLen);

                // Copy the byte array into a string.
                stringChain = Encoding.ASCII.GetString(input);
                string[] splittedFasta = stringChain.Split('\n');
                stringChain = "";
                String fastaHeader = splittedFasta[0];
                for (int j = 1; j < splittedFasta.Length; j++)
                {
                    stringChain += splittedFasta[j];
                }
                splittedFasta = stringChain.Split('\0');
                stringChain = "";

                for (int k = 0; k < splittedFasta.Length; k++)
                {
                    stringChain += splittedFasta[k];
                }

                splittedFasta = stringChain.Split('\t');
                stringChain = "";

                for (int l = 0; l < splittedFasta.Length; l++)
                {
                    stringChain += splittedFasta[l];
                }

                BaseChain libiadaChain = new BaseChain(stringChain);
                bool continueImport = db.matter.Any(m => m.name == matter.name);
                int i = 0;
                chain result;
                if (!continueImport)
                {
                    result = new chain();
                    result.dissimilar = false;
                    result.building_type_id = 1;
                    result.notation_id = notationId;
                    result.matter = matter;
                    result.creation_date = new DateTimeOffset(DateTime.Now);
                    for (int j = 0; j < libiadaChain.Alphabet.Power; j++)
                    {
                        alphabet alphabetElement = new alphabet();
                        alphabetElement.chain = result;
                        alphabetElement.number = j + 1;
                        String strElem = libiadaChain.Alphabet[j].ToString();
                        if (!db.element.Any(e => e.notation_id == notationId && e.value.Equals(strElem)))
                        {
                            TempData["failedElement"] = strElem;
                            return RedirectToAction("ImportFailure");
                        }
                        alphabetElement.element = db.element.Single(e => e.notation_id == notationId && e.value.Equals(strElem));
                        db.alphabet.AddObject(alphabetElement);
                    }
                    dna_information dnaData = new dna_information();
                    dnaData.matter = matter;
                    dnaData.fasta_header = fastaHeader;

                    db.dna_information.AddObject(dnaData);
                    db.chain.AddObject(result);
                    db.matter.AddObject(matter);
                    db.SaveChanges();
                }
                else
                {
                    long matterId = db.matter.Single(m => m.name == matter.name).id;
                    result = db.chain.Single(c => c.matter_id == matterId);
                    i = db.building.Where(b => b.chain_id == result.id).Select(b => b.index).Max() + 1;
                }

                int[] build = libiadaChain.Building;
                for (; i < build.Length; i++)
                {
                    building buildingElement = new building();
                    buildingElement.chain = result;
                    buildingElement.index = i;
                    buildingElement.number = build[i];
                    db.building.AddObject(buildingElement);
                    if (i % 1000 == 0)
                    {
                        db.SaveChanges();
                    }
                }

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

        public ActionResult ImportFailure()
        {
            ViewBag.failedElement = (String)TempData["failedElement"];
            return View();
        }
    }
}