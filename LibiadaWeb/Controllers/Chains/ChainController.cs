using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
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
                    {"remoteDbs", new SelectList(db.remote_db, "id", "name")},
                    {"natureLiterature", Aliases.NatureLiterature}
                };
            return View();
        }

        //
        // GET: /Chain/Segmentated

        public ActionResult Segmentated()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
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
                    String webApiId = String.Empty;
                    // Initialize the stream
                    Stream fileStream;
                    if (localFile)
                    {
                        var file = Request.Files[0];

                        if (file == null || file.ContentLength == 0)
                        {
                            throw new ArgumentNullException("Файл цепочки не задан или пуст");
                        }
                        fileStream = file.InputStream;
                    }
                    else
                    {
                        webApiId = NcbiHelper.GetId(chain.remote_id);
                        fileStream = NcbiHelper.GetFile(webApiId);
                    }
                   
                    var input = new byte[fileStream.Length];

                    

                    // Read the file into the byte array
                    fileStream.Read(input, 0, (int)fileStream.Length);
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
                                throw new Exception("В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                            }

                            alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, chain.notation_id, false);
                            dnaChainRepository.Insert(chain, fastaHeader, Convert.ToInt32(webApiId), alphabet, libiadaChain.Building);
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

                            literatureChainRepository.Insert(chain, original, languageId, alphabet, libiadaChain.Building);
                            break;
                    }
                }
                catch (Exception e)
                {
                    ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
                    ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
                    ViewBag.language_id = new SelectList(db.language, "id", "name", languageId);
                    ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", chain.piece_type_id);
                    ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", chain.remote_db_id);
                    ModelState.AddModelError("Error", e.Message);
                    return View(chain);
                }
                return RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            ViewBag.language_id = new SelectList(db.language, "id", "name", languageId);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", chain.remote_db_id);
            return View(chain);
        }

        //
        // POST: /Chain/Segmentated

        [HttpPost]
        public ActionResult Segmentated(chain chain, String stringChain)
        {
            if (ModelState.IsValid)
            {
                //отделяем заголовок fasta файла от цепочки
                string[] splittedChain = stringChain.Split('-');
                BaseChain libiadaChain = new BaseChain(splittedChain.Length);
                for (int k = 0; k < splittedChain.Length; k++)
                {
                    libiadaChain.Add(new ValueString(splittedChain[k]), k);
                }

                dna_chain dbDnaChain = new dna_chain
                {
                    matter_id = chain.matter_id,
                    notation_id = Aliases.NotationSegmented,
                    fasta_header = "",
                    piece_type_id = Aliases.PieceTypeFullGenome,
                    piece_position = 0,
                    created = DateTime.Now
                };

                long[] alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, dbDnaChain.notation_id, true);

                dnaChainRepository.Insert(dbDnaChain, alphabet, libiadaChain.Building);

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