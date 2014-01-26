using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    public class ChainController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly ChainRepository chainRepository;
        private readonly ElementRepository elementRepository;
        private readonly DnaChainRepository dnaChainRepository;
        private readonly LiteratureChainRepository literatureChainRepository;
        private readonly MatterRepository matterRepository;
        private readonly PieceTypeRepository pieceTypeRepository;
        private readonly NotationRepository notationRepository;
        private readonly RemoteDbRepository remoteDbRepository;

        public ChainController()
        {
            db = new LibiadaWebEntities();
            chainRepository = new ChainRepository(db);
            elementRepository = new ElementRepository(db);
            dnaChainRepository = new DnaChainRepository(db);
            literatureChainRepository = new LiteratureChainRepository(db);
            matterRepository = new MatterRepository(db);
            pieceTypeRepository = new PieceTypeRepository(db);
            notationRepository = new NotationRepository(db);
            remoteDbRepository = new RemoteDbRepository(db);
        }

        //
        // GET: /Chain/

        public ActionResult Index()
        {
            var chain = db.chain.OrderBy(c => c.created).Include("matter").Include("notation");
            return View(chain.ToList());
        }

        //
        // GET: /Chain/Details/5

        public ActionResult Details(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
            if (chain == null)
            {
                return HttpNotFound();
            }
            Chain libiadaChain = chainRepository.ToLibiadaChain(id);

            ViewBag.stringChain = libiadaChain.ToString();
            return View(chain);
        }

        //
        // GET: /Chain/Create

        public ActionResult Create()
        {  
            ViewBag.data = new Dictionary<string, object>
                {
                    {"matters", matterRepository.GetSelectListWithNature()},
                    {"notations", notationRepository.GetSelectListWithNature()},
                    {"languages", new SelectList(db.language, "id", "name")},
                    {"pieceTypes", pieceTypeRepository.GetSelectListWithNature()},
                    {"remoteDbs", remoteDbRepository.GetSelectListWithNature()},
                    {"natures", new SelectList(db.nature, "id", "name")},
                    {"natureLiterature", Aliases.NatureLiterature}
                };
            return View();
        }

        //
        // POST: /Chain/Create

        [HttpPost]
        public ActionResult Create(chain chain, bool localFile, int languageId, bool original)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int? webApiId = null;
                    // Initialize the stream
                    Stream fileStream;
                    if (localFile)
                    {
                        HttpPostedFileBase file = Request.Files[0];

                        if (file == null || file.ContentLength == 0)
                        {
                            throw new ArgumentNullException("Файл цепочки не задан или пуст");
                        }
                        fileStream = file.InputStream;
                    }
                    else
                    {
                        webApiId = NcbiHelper.GetId(chain.remote_id);
                        fileStream = NcbiHelper.GetFile(webApiId.ToString());
                    }

                    var input = new byte[fileStream.Length];



                    // Read the file into the byte array
                    fileStream.Read(input, 0, (int) fileStream.Length);
                    int natureId = db.matter.Single(m => m.id == chain.matter_id).nature_id;
                    // Copy the byte array into a string
                    string stringChain = natureId == Aliases.NatureGenetic
                        ? Encoding.ASCII.GetString(input)
                        : Encoding.UTF8.GetString(input);

                    BaseChain libiadaChain;
                    long[] alphabet;
                    switch (natureId)
                    {
                        case Aliases.NatureGenetic:
                            //отделяем заголовок fasta файла от цепочки
                            string[] splittedFasta = stringChain.Split(new[] {'\n', '\r'});
                            var chainStringBuilder = new StringBuilder();
                            String fastaHeader = splittedFasta[0];
                            for (int j = 1; j < splittedFasta.Length; j++)
                            {
                                chainStringBuilder.Append(splittedFasta[j]);
                            }

                            string resultStringChain = DataTransformers.CleanFastaFile(chainStringBuilder.ToString());

                            libiadaChain = new BaseChain(resultStringChain);

                            if (!elementRepository.ElementsInDb(libiadaChain.Alphabet, chain.notation_id))
                            {
                                throw new Exception(
                                    "В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                            }

                            alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, chain.notation_id, false);
                            dnaChainRepository.Insert(chain, fastaHeader, webApiId, alphabet, libiadaChain.Building);
                            break;
                        case Aliases.NatureMusic:
                            break;
                        case Aliases.NatureLiterature:
                            string[] text = stringChain.Split('\n');
                            for (int l = 0; l < text.Length - 1; l++)
                            {
                                // убираем \r
                                text[l] = text[l].Substring(0, text[l].Length - 1);
                            }

                            libiadaChain = new BaseChain(text.Length - 1);
                            // в конце файла всегда пустая строка поэтому последний элемент не считаем
                            //TODO: переделать этот говнокод и вообще добавить проверку на пустую строку в конце а лучше сделать нормальный trim
                            for (int i = 0; i < text.Length - 1; i++)
                            {
                                libiadaChain.Add(new ValueString(text[i]), i);
                            }

                            alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, chain.notation_id, true);

                            literatureChainRepository.Insert(chain, original, languageId, alphabet,
                                libiadaChain.Building);
                            break;
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("Error", e.Message);
                }

            }

            ViewBag.data = new Dictionary<string, object>
            {
                {"matters", matterRepository.GetSelectListWithNature(chain.matter_id)},
                {"notations", notationRepository.GetSelectListWithNature(chain.notation_id)},
                {"languages", new SelectList(db.language, "id", "name", languageId)},
                {"pieceTypes", pieceTypeRepository.GetSelectListWithNature(chain.piece_type_id)},
                {"remoteDbs", chain.remote_db_id == null
                        ? remoteDbRepository.GetSelectListWithNature()
                        : remoteDbRepository.GetSelectListWithNature((int) chain.remote_db_id)},
                {"natures", new SelectList(db.nature, "id", "name", chain.matter.nature_id)},
                {"natureLiterature", Aliases.NatureLiterature}
            };
            return View(chain);
        }

        //
        // GET: /Chain/Segmentated

        public ActionResult Segmentated()
        {
            ViewBag.data = new Dictionary<string, object>
            {
                {"matters", new SelectList(db.matter.Where(m => m.nature_id == Aliases.NatureGenetic), "id", "name")},
                {"notations", new SelectList(db.notation.Where(n => n.nature_id == Aliases.NatureGenetic), "id", "name")},
                {"pieceTypes",  new SelectList(db.piece_type.Where(p => p.nature_id ==Aliases.NatureGenetic), "id", "name")}
            };
            return View();
        }

        //
        // POST: /Chain/Segmentated

        [HttpPost]
        public ActionResult Segmentated(chain chain, String stringChain)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //отделяем заголовок fasta файла от цепочки
                    string[] splittedChain = stringChain.Split('-');
                    var libiadaChain = new BaseChain(splittedChain.Length);
                    for (int k = 0; k < splittedChain.Length; k++)
                    {
                        libiadaChain.Add(new ValueString(splittedChain[k]), k);
                    }

                    long[] alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, chain.notation_id, true);

                    dnaChainRepository.Insert(chain, null, null, alphabet, libiadaChain.Building);

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("Error", e.Message);
                }
            }

            ViewBag.data = new Dictionary<string, object>
            {
                {"matters", new SelectList(db.matter.Where(m => m.nature_id == Aliases.NatureGenetic), "id", "name", chain.matter_id)},
                {"notations", new SelectList(db.notation.Where(n => n.nature_id == Aliases.NatureGenetic), "id", "name", chain.notation_id)},
                {"pieceTypes",  new SelectList(db.piece_type.Where(p => p.nature_id ==Aliases.NatureGenetic), "id", "name", chain.piece_type_id)}
            };
            return View(chain);
        }

        //
        // GET: /Chain/Edit/5

        public ActionResult Edit(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
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

        //
        // POST: /Chain/Edit/5

        [HttpPost]
        public ActionResult Edit(chain chain)
        {
            if (ModelState.IsValid)
            {
                db.chain.Attach(chain);
                db.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", chain.remote_db_id);
            return View(chain);
        }

        //
        // GET: /Chain/Delete/5

        public ActionResult Delete(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
            if (chain == null)
            {
                return HttpNotFound();
            }
            return View(chain);
        }

        //
        // POST: /Chain/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
            db.chain.DeleteObject(chain);
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