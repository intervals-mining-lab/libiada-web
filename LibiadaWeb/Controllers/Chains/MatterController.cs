namespace LibiadaWeb.Controllers.Chains
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web.Mvc;
    using System.Xml;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using Helpers;
    using Models;
    using Models.Repositories.Catalogs;
    using Models.Repositories.Chains;

    /// <summary>
    /// The matter controller.
    /// </summary>
    public class MatterController : Controller
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
        /// Initializes a new instance of the <see cref="MatterController"/> class.
        /// </summary>
        public MatterController()
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

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            var matter = db.matter.Include(m => m.nature);
            return View(matter.ToList());
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

            matter matter = db.matter.Find(id);
            if (matter == null)
            {
                return HttpNotFound();
            }

            return View(matter);
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
            ViewBag.data = new Dictionary<string, object>
                {
                    { "notations", notationRepository.GetSelectListWithNature() },
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature() },
                    { "languages", new SelectList(db.language, "id", "name") },
                    { "remoteDbs", remoteDbRepository.GetSelectListWithNature() },
                    { "natures", new SelectList(db.nature, "id", "name") },
                    { "translators", new SelectList(db.translator, "id", "name") },
                    { "natureLiterature", Aliases.NatureLiterature },
                    { "natureGenetic", Aliases.NatureGenetic }
                };
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <param name="notationId">
        /// The notation Id.
        /// </param>
        /// <param name="pieceTypeId">
        /// The piece Type Id.
        /// </param>
        /// <param name="remoteDbId">
        /// The remote Db Id.
        /// </param>
        /// <param name="remoteId">
        /// The remote Id.
        /// </param>
        /// <param name="localFile">
        /// The local File.
        /// </param>
        /// <param name="languageId">
        /// The language Id.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="translatorId">
        /// The translator Id.
        /// </param>
        /// <param name="productId">
        /// The product Id.
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "name,nature_id,description")] matter matter,
            int notationId,
            int pieceTypeId,
            int? remoteDbId,
            string remoteId,
            bool localFile,
            int? languageId,
            bool? original,
            int? translatorId,
            int? productId,
            bool? partial,
            bool? complement)
        {
            if (ModelState.IsValid)
            {
                bool matterCreated = false;
                try
                {
                    int? webApiId = null;

                    Stream fileStream;
                    if (localFile)
                    {
                        var file = Request.Files[0];

                        if (file == null || file.ContentLength == 0)
                        {
                            throw new ArgumentNullException("Chain file is null or empty");
                        }

                        fileStream = file.InputStream;
                    }
                    else
                    {
                        webApiId = NcbiHelper.GetId(remoteId);
                        fileStream = NcbiHelper.GetFile(webApiId.ToString());
                    }

                    var input = new byte[fileStream.Length];

                    // Read the file into the byte array
                    fileStream.Read(input, 0, (int)fileStream.Length);

                    string stringChain = matter.nature_id == Aliases.NatureGenetic
                        ? Encoding.ASCII.GetString(input)
                        : Encoding.UTF8.GetString(input);

                    BaseChain libiadaChain;
                    long[] alphabet;

                    switch (matter.nature_id)
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

                            if (!elementRepository.ElementsInDb(libiadaChain.Alphabet, notationId))
                            {
                                throw new Exception("At least one element of new chain missing in db.");
                            }

                            db.matter.Add(matter);
                            db.SaveChanges();

                            matterCreated = true;

                            var dnaChain = new chain
                            {
                                notation_id = notationId,
                                piece_type_id = pieceTypeId,
                                matter_id = matter.id,
                                remote_db_id = remoteDbId,
                                remote_id = remoteId
                            };

                            alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, dnaChain.notation_id, false);
                            dnaChainRepository.Insert(dnaChain, fastaHeader, webApiId, productId, (bool)complement, (bool)partial, alphabet, libiadaChain.Building);
                            break;
                        case Aliases.NatureMusic:
                            var doc = new XmlDocument();
                            doc.LoadXml(stringChain);
                            //MusicXmlParser parser = new MusicXmlParser();
                            //parser.Execute(doc, "test");
                            //ScoreTrack tempTrack = parser.ScoreModel;

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
                            for (int i = 0; i < text.Length - 1; i++)
                            {
                                libiadaChain.Set(new ValueString(text[i]), i);
                            }

                            db.matter.Add(matter);
                            db.SaveChanges();

                            var literatureChain = new chain
                            {
                                notation_id = notationId,
                                piece_type_id = pieceTypeId,
                                matter_id = matter.id,
                                remote_db_id = remoteDbId,
                                remote_id = remoteId
                            };

                            alphabet = elementRepository.ToDbElements(
                                libiadaChain.Alphabet,
                                literatureChain.notation_id,
                                true);

                            literatureChainRepository.Insert(
                                literatureChain,
                                (bool)original,
                                (int)languageId,
                                translatorId,
                                alphabet,
                                libiadaChain.Building);

                            break;
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    string error = (matterCreated
                        ? "Объект исследования успешно добавлен в БД, однако при создании цепочки произошла ошибка: "
                        : "Не удалось создать объект исследования: ") + e.Message;

                    ModelState.AddModelError("Error", error);
                }
            }

            ViewBag.data = new Dictionary<string, object>
                    {
                        { "notations", notationRepository.GetSelectListWithNature(notationId) },
                        { "pieceTypes", pieceTypeRepository.GetSelectListWithNature(pieceTypeId) },
                        { "languages", new SelectList(db.language, "id", "name", languageId) },
                        { "remoteDbs", remoteDbId == null ? 
                            remoteDbRepository.GetSelectListWithNature() : 
                            remoteDbRepository.GetSelectListWithNature((int)remoteDbId) },
                        { "natures", new SelectList(db.nature, "id", "name", matter.nature_id) },
                        { "translators", new SelectList(db.translator, "id", "name") },
                        { "natureLiterature", Aliases.NatureLiterature },
                        { "natureGenetic", Aliases.NatureGenetic }
                    };
            return View(matter);
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

            matter matter = db.matter.Find(id);
            if (matter == null)
            {
                return HttpNotFound();
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "name,nature_id,description")] matter matter)
        {
            if (ModelState.IsValid)
            {
                db.Entry(matter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            return View(matter);
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

            matter matter = db.matter.Find(id);
            if (matter == null)
            {
                return HttpNotFound();
            }

            return View(matter);
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
            matter matter = db.matter.Find(id);
            db.matter.Remove(matter);
            db.SaveChanges();
            return RedirectToAction("Index");
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
