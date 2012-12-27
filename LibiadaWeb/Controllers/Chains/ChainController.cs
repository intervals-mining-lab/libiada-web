using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{ 
    public class ChainController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly DnaChainRepository dnaChainRepository;
        private readonly LiteratureChainRepository literatureChainRepository;
        private readonly ChainRepository chainRepository;
        private readonly AlphabetRepository alphabetRepository;

        public ChainController()
        {
            dnaChainRepository = new DnaChainRepository(db);
            literatureChainRepository = new LiteratureChainRepository(db);
            chainRepository = new ChainRepository(db);
            alphabetRepository = new AlphabetRepository(db);
        }

        //
        // GET: /Chain/

        public ViewResult Index()
        {
            var chain = db.chain.OrderBy(c => c.creation_date).Include("matter").Include("notation");
            return View(chain.ToList());
        }

        //
        // GET: /Chain/Details/5

        public ViewResult Details(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
            Chain libiadaChain = chainRepository.FromDbChainToLibiadaChain(id);

            ViewBag.stringChain = libiadaChain.ToString();
            return View(chain);
        }

        //
        // GET: /Chain/Create

        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
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
            return View();
        }

        //
        // POST: /Chain/Create

        [HttpPost]
        public ActionResult Segmentated(chain chain, String stringChain)
        {
            if (ModelState.IsValid)
            {
                BaseChain libiadaChain = new BaseChain(stringChain);

                //отделяем заголовок fasta файла от цепочки
                string[] splittedChain = stringChain.Split('-');
                libiadaChain = new BaseChain(splittedChain.Length);
                for (int k = 0; k < splittedChain.Length; k++)
                {
                    libiadaChain.Add(new ValueString(splittedChain[k]), k);
                }
                dna_chain dbDnaChain;

                dbDnaChain = new dna_chain()
                    {
                        id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                        dissimilar = false,
                        notation_id = 6,
                        fasta_header = "",
                        piece_type_id = 1,
                        piece_position = 0,
                        creation_date = DateTime.Now
                    };

                db.matter.Single(m => m.id == chain.matter_id).dna_chain.Add(dbDnaChain);
                //TODO: проверить, возможно одно из действий лишнее
                db.dna_chain.AddObject(dbDnaChain);
                alphabetRepository.FromLibiadaAlphabetToDbAlphabet(libiadaChain.Alphabet, dbDnaChain.notation_id,
                                                                       dbDnaChain.id, true);
                dnaChainRepository.FromLibiadaBuildingToDbBuilding(dbDnaChain, libiadaChain.Building);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            return View(chain);
        }

        //
        // POST: /Chain/Create

        [HttpPost]
        public ActionResult Create(chain chain, int languageId, bool original)
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
                int natureId = db.matter.Single(m => m.id == chain.matter_id).nature_id;
                // Copy the byte array into a string
                String stringChain;
                if (natureId == 1)
                {
                    stringChain = Encoding.ASCII.GetString(input);
                }
                else
                {
                    stringChain = Encoding.UTF8.GetString(input);
                }

                BaseChain libiadaChain;// = new BaseChain(stringChain);
                int[] libiadaBuilding;
                switch (natureId)
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
                        bool continueImport =
                            db.dna_chain.Any(
                                d =>
                                d.notation_id == chain.notation_id && d.matter_id == chain.matter_id &&
                                d.fasta_header == fastaHeader); 
                        if (!continueImport)
                        {
                            dbDnaChain = new dna_chain()
                            {
                                id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                                dissimilar = chain.dissimilar,
                                notation_id = chain.notation_id,
                                fasta_header = fastaHeader,
                                piece_type_id = chain.piece_type_id,
                                creation_date = DateTime.Now
                            };

                            db.matter.Single(m => m.id == chain.matter_id).dna_chain.Add(dbDnaChain); //TODO: проверить, возможно одно из действий лишнее
                            db.dna_chain.AddObject(dbDnaChain);
                            IEnumerable<alphabet> alphabet = alphabetRepository.FromLibiadaAlphabetToDbAlphabet(libiadaChain.Alphabet, chain.notation_id, dbDnaChain.id, false);
                             
                        }
                        else
                        {
                            dbDnaChain = db.dna_chain.Single(c => c.matter_id == chain.matter_id && c.notation_id == chain.notation_id && c.fasta_header == fastaHeader);
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
                        continueImport =
                            db.literature_chain.Any(
                                d =>
                                d.notation_id == chain.notation_id && d.matter_id == chain.matter_id &&
                                d.language_id == languageId); 
                        if (!continueImport)
                        {

                            dbLiteratureChain = new literature_chain()
                            {
                                id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                                dissimilar = chain.dissimilar,
                                notation_id = chain.notation_id,
                                language_id = languageId,
                                original = original,
                                piece_type_id = chain.piece_type_id,
                                creation_date = DateTime.Now
                            };

                            db.matter.Single(m => m.id == chain.matter_id).literature_chain.Add(dbLiteratureChain); //TODO: проверить, возможно одно из действий лишнее
                            db.literature_chain.AddObject(dbLiteratureChain);

                            IEnumerable<alphabet> alphabet = alphabetRepository.FromLibiadaAlphabetToDbAlphabet(libiadaChain.Alphabet, chain.notation_id, dbLiteratureChain.id, true);
                        }
                        else
                        {
                            dbLiteratureChain = db.literature_chain.Single(c => c.matter_id == chain.matter_id && c.notation_id == chain.notation_id && c.language_id == languageId);
                        }

                        libiadaBuilding = libiadaChain.Building;

                        literatureChainRepository.FromLibiadaBuildingToDbBuilding(dbLiteratureChain, libiadaBuilding);

                        db.SaveChanges();
                        break;
                }

                return RedirectToAction("Index");  
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            return View(chain);
        }
        
        //
        // GET: /Chain/Edit/5
 
        public ActionResult Edit(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
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
            return View(chain);
        }

        //
        // GET: /Chain/Delete/5
 
        public ActionResult Delete(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
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