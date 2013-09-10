using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    public class MatterController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly AlphabetRepository alphabetRepository;
        private readonly BuildingRepository buildingRepository;
        private readonly ElementRepository elementRepository;

        public MatterController()
        {
            alphabetRepository = new AlphabetRepository(db);
            buildingRepository = new BuildingRepository(db);
            elementRepository = new ElementRepository(db);
        }

        //
        // GET: /Matter/

        public ActionResult Index()
        {
            var matter = db.matter.Include("nature").Include("remote_db");
            return View(matter.ToList());
        }

        //
        // GET: /Matter/Details/5

        public ActionResult Details(long id)
        {
            matter matter = db.matter.Single(m => m.id == id);
            if (matter == null)
            {
                return HttpNotFound();
            }
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
                if (fileLen == 0)
                {
                    ViewBag.nature_id = new SelectList(db.nature, "id", "name");
                    ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
                    ViewBag.notation_id = new SelectList(db.notation, "id", "name");
                    ViewBag.language_id = new SelectList(db.language, "id", "name");
                    ModelState.AddModelError("Error", "Файл цепочки не задан");
                    return View(matter);
                }

                byte[] input = new byte[fileLen];

                // Initialize the stream
                var fileStream = file.InputStream;

                // Read the file into the byte array
                fileStream.Read(input, 0, fileLen);

                string stringChain = string.Empty;
                // Copy the byte array into a string
                stringChain = matter.nature_id == Aliases.NatureGenetic
                                  ? Encoding.ASCII.GetString(input)
                                  : Encoding.UTF8.GetString(input);

                BaseChain libiadaChain;
                bool continueImport = db.matter.Any(m => m.name == matter.name);
                switch (matter.nature_id)
                {
                        //генетическая цепочка
                    case 1:
                        //отделяем заголовок fasta файла от цепочки
                        string[] splittedFasta = stringChain.Split(new [] { '\n' , '\r' });
                        StringBuilder chainStringBuilder = new StringBuilder();
                        String fastaHeader = splittedFasta[0];
                        for (int j = 1; j < splittedFasta.Length; j++)
                        {
                            chainStringBuilder.Append(splittedFasta[j]);
                        }

                        string resultStringChain = DataTransformators.CleanFastaFile(chainStringBuilder.ToString());

                        libiadaChain = new BaseChain(resultStringChain);
                        dna_chain dbDnaChain;
                        if (!continueImport)
                        {
                            if (!elementRepository.ElementsInDb(libiadaChain.Alphabet, notationId))
                            {
                                ViewBag.nature_id = new SelectList(db.nature, "id", "name");
                                ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
                                ViewBag.notation_id = new SelectList(db.notation, "id", "name");
                                ViewBag.language_id = new SelectList(db.language, "id", "name");
                                ModelState.AddModelError("Error", "В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                                return View(matter);
                            }
                            db.matter.AddObject(matter);

                            dbDnaChain = new dna_chain
                                {
                                    id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                                    dissimilar = false,
                                    notation_id = notationId,
                                    fasta_header = fastaHeader,
                                    piece_type_id = 1,
                                    creation_date = new DateTimeOffset(DateTime.Now)
                                };

                            matter.dna_chain.Add(dbDnaChain);

                            alphabetRepository.ToDbAlphabet(libiadaChain.Alphabet, notationId,
                                                                               dbDnaChain.id, false);
                        }
                        else
                        {
                            dbDnaChain =
                                db.dna_chain.Single(
                                    c =>
                                    c.matter_id == matter.id && c.notation_id == notationId &&
                                    c.fasta_header == fastaHeader);
                        }

                        buildingRepository.ToDbBuilding(dbDnaChain.id, libiadaChain.Building);

                        db.SaveChanges();
                        break;
                        //музыкальная цепочка
                    case 2:
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(stringChain);
                        //MusicXmlParser parser = new MusicXmlParser();
                        //parser.Execute(doc, "test");
                        //ScoreTrack tempTrack = parser.ScoreModel;

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

                            dbLiteratureChain = new literature_chain
                                {
                                    id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                                    dissimilar = false,
                                    notation_id = notationId,
                                    language_id = languageId,
                                    original = original,
                                    piece_type_id = 1,
                                    creation_date = new DateTimeOffset(DateTime.Now)
                                };

                            matter.literature_chain.Add(dbLiteratureChain);

                            alphabetRepository.ToDbAlphabet(libiadaChain.Alphabet, notationId,
                                                                               dbLiteratureChain.id, true);
                        }
                        else
                        {
                            dbLiteratureChain =
                                db.literature_chain.Single(
                                    c =>
                                    c.matter_id == matter.id && c.notation_id == notationId &&
                                    c.language_id == languageId);
                        }

                        buildingRepository.ToDbBuilding(dbLiteratureChain.id, libiadaChain.Building);

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
            if (matter == null)
            {
                return HttpNotFound();
            }
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
            if (matter == null)
            {
                return HttpNotFound();
            }
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
            ViewBag.failedElement = TempData["failedElement"];
            return View();
        }
    }
}