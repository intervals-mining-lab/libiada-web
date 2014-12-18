namespace LibiadaWeb.Controllers.Chains
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The chain controller.
    /// </summary>
    public class ChainController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// The dna chain repository.
        /// </summary>
        private readonly DnaChainRepository dnaChainRepository;

        /// <summary>
        /// The literature chain repository.
        /// </summary>
        private readonly LiteratureChainRepository literatureChainRepository;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The piece type repository.
        /// </summary>
        private readonly PieceTypeRepository pieceTypeRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The remote db repository.
        /// </summary>
        private readonly RemoteDbRepository remoteDbRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainController"/> class.
        /// </summary>
        public ChainController()
        {
            db = new LibiadaWebEntities();
            this.chainRepository = new ChainRepository(db);
            this.elementRepository = new ElementRepository(db);
            this.dnaChainRepository = new DnaChainRepository(db);
            this.literatureChainRepository = new LiteratureChainRepository(db);
            this.matterRepository = new MatterRepository(db);
            this.pieceTypeRepository = new PieceTypeRepository(db);
            this.notationRepository = new NotationRepository(db);
            this.remoteDbRepository = new RemoteDbRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            var chain = db.chain.Include(c => c.matter).
                                 Include(c => c.notation).
                                 Include(c => c.piece_type).
                                 Include(c => c.remote_db);
            return View(chain.ToList());
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            chain chain = db.chain.Find(id);
            if (chain == null)
            {
                return this.HttpNotFound();
            }

            return View(chain);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);

            var translators = new SelectList(db.translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", this.matterRepository.GetSelectListWithNature() }, 
                    { "notations", this.notationRepository.GetSelectListWithNature() }, 
                    { "languages", new SelectList(db.language, "id", "name") }, 
                    { "pieceTypes", this.pieceTypeRepository.GetSelectListWithNature() }, 
                    { "remoteDbs", this.remoteDbRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(db.nature, "id", "name") }, 
                    { "translators", translators }, 
                    { "natureLiterature", Aliases.NatureLiterature }, 
                    { "natureGenetic", Aliases.NatureGenetic }
                };
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="chain">
        /// The chain.
        /// </param>
        /// <param name="localFile">
        /// The local file.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="translatorId">
        /// The translator id.
        /// </param>
        /// <param name="productId">
        /// The product id.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <param name="complement">
        /// The complement.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "notation_id,matter_id,dissimilar,piece_type_id,piece_position,remote_db_id,remote_id,description")] chain chain,
            bool localFile,
            int languageId,
            bool original,
            int? translatorId,
            int? productId,
            bool partial,
            bool complement)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    int? webApiId = null;

                    Stream fileStream;
                    if (localFile)
                    {
                        HttpPostedFileBase file = this.Request.Files[0];

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

                            // отделяем заголовок fasta файла от цепочки
                            string[] splittedFasta = stringChain.Split(new[] { '\n', '\r' });
                            var chainStringBuilder = new StringBuilder();
                            string fastaHeader = splittedFasta[0];
                            for (int j = 1; j < splittedFasta.Length; j++)
                            {
                                chainStringBuilder.Append(splittedFasta[j]);
                            }

                            string resultStringChain = DataTransformers.CleanFastaFile(chainStringBuilder.ToString());

                            libiadaChain = new BaseChain(resultStringChain);

                            if (!this.elementRepository.ElementsInDb(libiadaChain.Alphabet, chain.notation_id))
                            {
                                throw new Exception("В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                            }

                            alphabet = this.elementRepository.ToDbElements(
                                libiadaChain.Alphabet, 
                                chain.notation_id, 
                                false);
                            this.dnaChainRepository.Insert(
                                chain, 
                                fastaHeader, 
                                webApiId, 
                                productId, 
                                complement, 
                                partial, 
                                alphabet, 
                                libiadaChain.Building);
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
                            // TODO: переделать этот говнокод и вообще добавить проверку на пустую строку в конце а лучше сделать нормальный trim
                            for (int i = 0; i < text.Length - 1; i++)
                            {
                                libiadaChain.Set(new ValueString(text[i]), i);
                            }

                            alphabet = this.elementRepository.ToDbElements(
                                libiadaChain.Alphabet, 
                                chain.notation_id, 
                                true);

                            this.literatureChainRepository.Insert(
                                chain, 
                                original, 
                                languageId, 
                                translatorId, 
                                alphabet, 
                                libiadaChain.Building);
                            break;
                    }

                    return this.RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    this.ModelState.AddModelError("Error", e.Message);
                }
            }

            var translators = new SelectList(db.translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
            {
                { "matters", this.matterRepository.GetSelectListWithNature(chain.matter_id) }, 
                { "notations", this.notationRepository.GetSelectListWithNature(chain.notation_id) }, 
                { "languages", new SelectList(db.language, "id", "name", languageId) }, 
                { "pieceTypes", this.pieceTypeRepository.GetSelectListWithNature(chain.piece_type_id) }, 
                { "remoteDbs", chain.remote_db_id == null
                        ? remoteDbRepository.GetSelectListWithNature()
                        : remoteDbRepository.GetSelectListWithNature((int)chain.remote_db_id) }, 
                { "natures", new SelectList(db.nature, "id", "name", db.matter.Single(m => m.id == chain.matter_id).nature_id) }, 
                { "natureLiterature", Aliases.NatureLiterature },
                { "natureGenetic", Aliases.NatureGenetic },
                    { "translators", translators }
            };
            return View(chain);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            chain chain = db.chain.Find(id);
            if (chain == null)
            {
                return this.HttpNotFound();
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", chain.remote_db_id);
            return View(chain);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="chain">
        /// The chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "notation_id,matter_id,dissimilar,piece_type_id,piece_position,remote_db_id,remote_id,description")] chain chain)
        {
            if (this.ModelState.IsValid)
            {
                db.Entry(chain).State = EntityState.Modified;
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", chain.remote_db_id);
            return View(chain);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            chain chain = db.chain.Find(id);
            if (chain == null)
            {
                return this.HttpNotFound();
            }

            return View(chain);
        }

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            chain chain = db.chain.Find(id);
            db.chain.Remove(chain);
            db.SaveChanges();
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
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
