using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaWeb;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using DownLoadedFile = System.IO.File;

namespace LibiadaWeb.Controllers
{
    public class MatterController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();
        private ChainRepository chainRepository = new ChainRepository();
        private LiteratureChainRepository literatureChainRepository ;
        private DnaChainRepository dnaChainRepository = new DnaChainRepository();

        public MatterController()
        {
            literatureChainRepository = new LiteratureChainRepository(db);
        }

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
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            return View();
        }

        //
        // POST: /Matter/Create

        [HttpPost]
        public ActionResult Create(matter matter, int notationId, int languageId, bool original)
        {

            if (ModelState.IsValid)
            {
                var file = Request.Files[0];

                int fileLen = file.ContentLength;
                byte[] input = new byte[fileLen];

                // Initialize the stream
                var fileStream = file.InputStream;

                // Read the file into the byte array
                fileStream.Read(input, 0, fileLen);

                string stringChain;
                // Copy the byte array into a string
                if(matter.nature_id == 1)
                {
                    stringChain = Encoding.ASCII.GetString(input);
                }
                else
                {
                    stringChain = Encoding.UTF8.GetString(input);
                }

                BaseChain libiadaChain;
                int[] libiadaBuilding;
                bool continueImport = db.matter.Any(m => m.name == matter.name);
                switch (matter.nature_id)
                {
                    //генетическая цепочка
                    case 1:
                        //отделяем заголовок fasta файла от цепочки
                        string[] splittedFasta = stringChain.Split('\n');
                        stringChain = "";
                        String fastaHeader = splittedFasta[0];
                        for (int j = 1; j < splittedFasta.Length; j++)
                        {
                            stringChain += splittedFasta[j];
                        }

                        stringChain = DataTransformators.CleanFastaFile(stringChain);

                        libiadaChain = new BaseChain(stringChain);
                        dna_chain dbDnaChain;
                        if (!continueImport)
                        {
                            db.matter.AddObject(matter);

                            dbDnaChain = new dna_chain()
                            {
                                id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                                dissimilar = false,
                                building_type_id = 1,
                                notation_id = notationId,
                                fasta_header = fastaHeader,
                                piece_type_id = 1,
                                creation_date = new DateTimeOffset(DateTime.Now)
                            };

                            matter.dna_chain.Add(dbDnaChain); //TODO: проверить, возможно одно из действий лишнее
                            db.dna_chain.AddObject(dbDnaChain);

                            dnaChainRepository.FromLibiadaAlphabetToDbAlphabet(libiadaChain.Alphabet, dbDnaChain, notationId);
                        }
                        else
                        {
                            long matterId = db.matter.Single(m => m.name == matter.name).id;
                            dbDnaChain = db.dna_chain.Single(c => c.matter_id == matterId);
                        }

                        libiadaBuilding = libiadaChain.Building;

                        dnaChainRepository.FromLibiadaBuildingToDbBuilding(dbDnaChain, libiadaBuilding);

                        db.SaveChanges();
                        break;
                    //музыкальная цепочка
                    case 2:
                        break;
                    //литературная цепочка
                    case 3:
                        string[] text = stringChain.Split('\n');
                        for (int l = 0; l < text.Length - 1; l++)
                        {
                            // убираем \r
                            text[l] = text[l].Substring(0, text[l].Length - 1);
                        }

                        libiadaChain = new BaseChain(text.Length - 1);
                        // в конце файла всегда пустая строка поэтому последний элемент не считаем
                        for (int i = 0; i < text.Length - 1; i++)
                        {
                            libiadaChain.Add(new ValueString(text[i]), i);
                        }
                        literature_chain dbLiteratureChain;
                        if (!continueImport)
                        {
                            db.matter.AddObject(matter);

                            dbLiteratureChain = new literature_chain()
                            {
                                id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                                dissimilar = false,
                                building_type_id = 1,
                                notation_id = notationId,
                                language_id = languageId,
                                original = original,
                                piece_type_id = 1,
                                creation_date = new DateTimeOffset(DateTime.Now)
                            };

                            matter.literature_chain.Add(dbLiteratureChain); //TODO: проверить, возможно одно из действий лишнее
                            db.literature_chain.AddObject(dbLiteratureChain);

                            literatureChainRepository.FromLibiadaAlphabetToDbAlphabet(libiadaChain.Alphabet, dbLiteratureChain, notationId);
                        }
                        else
                        {
                            long matterId = db.matter.Single(m => m.name == matter.name).id;
                            dbLiteratureChain = db.literature_chain.Single(c => c.matter_id == matterId);
                        }

                        libiadaBuilding = libiadaChain.Building;

                        literatureChainRepository.FromLibiadaBuildingToDbBuilding(dbLiteratureChain, libiadaBuilding);

                        db.SaveChanges();
                        break;
                }
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