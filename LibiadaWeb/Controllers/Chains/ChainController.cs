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

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using Models;
    using Models.Repositories.Catalogs;
    using Models.Repositories.Chains;

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
        /// The common sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// The dna sequence repository.
        /// </summary>
        private readonly DnaSequenceRepository dnaChainRepository;

        /// <summary>
        /// The literature sequence repository.
        /// </summary>
        private readonly LiteratureSequenceRepository literatureChainRepository;

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
            commonSequenceRepository = new CommonSequenceRepository(db);
            elementRepository = new ElementRepository(db);
            dnaChainRepository = new DnaSequenceRepository(db);
            literatureChainRepository = new LiteratureSequenceRepository(db);
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
            var chain = db.CommonSequence.Include(c => c.Matter)
                                .Include(c => c.Notation)
                                .Include(c => c.PieceType)
                                .Include(c => c.RemoteDb);
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

            CommonSequence sequence = db.CommonSequence.Find(id);
            if (sequence == null)
            {
                return HttpNotFound();
            }

            return View(sequence);
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

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetSelectListWithNature() }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "languages", new SelectList(db.Language, "id", "name") }, 
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature() }, 
                    { "remoteDbs", remoteDbRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(db.Nature, "id", "name") }, 
                    { "translators", translators }, 
                    { "natureLiterature", Aliases.Nature.Literature }, 
                    { "natureGenetic", Aliases.Nature.Genetic }
                };
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="sequence">
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
        /// Thrown if file is null.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if element of created chain is not found in db.
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "notation_id,matter_id,piece_type_id,piece_position,remote_db_id,remote_id,description")] CommonSequence sequence,
            bool localFile,
            int languageId,
            bool original,
            int? translatorId,
            int? productId,
            bool partial,
            bool complement)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int? webApiId = null;

                    Stream fileStream;
                    if (localFile)
                    {
                        HttpPostedFileBase file = Request.Files[0];

                        if (file == null || file.ContentLength == 0)
                        {
                            throw new ArgumentNullException("file", "File not attached or empty.");
                        }

                        fileStream = file.InputStream;
                    }
                    else
                    {
                        webApiId = NcbiHelper.GetId(sequence.RemoteId);
                        fileStream = NcbiHelper.GetFile(webApiId.ToString());
                    }

                    var input = new byte[fileStream.Length];

                    // Read the file into the byte array
                    fileStream.Read(input, 0, (int)fileStream.Length);
                    int natureId = db.Matter.Single(m => m.Id == sequence.MatterId).NatureId;

                    // Copy the byte array into a string
                    string stringChain = natureId == Aliases.Nature.Genetic
                        ? Encoding.ASCII.GetString(input)
                        : Encoding.UTF8.GetString(input);

                    BaseChain libiadaChain;
                    long[] alphabet;
                    switch (natureId)
                    {
                        case Aliases.Nature.Genetic:

                            // отделяем заголовок fasta файла от цепочки
                            string[] splittedFasta = stringChain.Split('\n', '\r');
                            var chainStringBuilder = new StringBuilder();
                            string fastaHeader = splittedFasta[0];
                            for (int j = 1; j < splittedFasta.Length; j++)
                            {
                                chainStringBuilder.Append(splittedFasta[j]);
                            }

                            string resultStringChain = DataTransformers.CleanFastaFile(chainStringBuilder.ToString());

                            libiadaChain = new BaseChain(resultStringChain);

                            if (!elementRepository.ElementsInDb(libiadaChain.Alphabet, sequence.NotationId))
                            {
                                throw new Exception("В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                            }

                            alphabet = elementRepository.ToDbElements(
                                libiadaChain.Alphabet,
                                sequence.NotationId,
                                false);
                            dnaChainRepository.Insert(
                                sequence,
                                fastaHeader,
                                webApiId,
                                productId,
                                complement,
                                partial,
                                alphabet,
                                libiadaChain.Building);
                            break;
                        case Aliases.Nature.Music:
                            break;
                        case Aliases.Nature.Literature:
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

                            alphabet = elementRepository.ToDbElements(
                                libiadaChain.Alphabet,
                                sequence.NotationId,
                                true);

                            literatureChainRepository.Insert(
                                sequence,
                                original,
                                languageId,
                                translatorId,
                                alphabet,
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

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
            {
                { "matters", matterRepository.GetSelectListWithNature(sequence.MatterId) }, 
                { "notations", notationRepository.GetSelectListWithNature(sequence.NotationId) }, 
                { "languages", new SelectList(db.Language, "id", "name", languageId) }, 
                { "pieceTypes", pieceTypeRepository.GetSelectListWithNature(sequence.PieceTypeId) }, 
                { "remoteDbs", sequence.RemoteDbId == null
                        ? remoteDbRepository.GetSelectListWithNature()
                        : remoteDbRepository.GetSelectListWithNature((int)sequence.RemoteDbId) }, 
                { "natures", new SelectList(db.Nature, "id", "name", db.Matter.Single(m => m.Id == sequence.MatterId).NatureId) }, 
                { "natureLiterature", Aliases.Nature.Literature },
                { "natureGenetic", Aliases.Nature.Genetic },
                    { "translators", translators }
            };
            return View(sequence);
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

            CommonSequence sequence = db.CommonSequence.Find(id);
            if (sequence == null)
            {
                return HttpNotFound();
            }

            ViewBag.matter_id = new SelectList(db.Matter, "id", "name", sequence.MatterId);
            ViewBag.notation_id = new SelectList(db.Notation, "id", "name", sequence.NotationId);
            ViewBag.piece_type_id = new SelectList(db.PieceType, "id", "name", sequence.PieceTypeId);
            ViewBag.remote_db_id = new SelectList(db.RemoteDb, "id", "name", sequence.RemoteDbId);
            return View(sequence);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="sequence">
        /// The chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "notation_id,matter_id,piece_type_id,piece_position,remote_db_id,remote_id,description")] CommonSequence sequence)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sequence).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.Matter, "id", "name", sequence.MatterId);
            ViewBag.notation_id = new SelectList(db.Notation, "id", "name", sequence.NotationId);
            ViewBag.piece_type_id = new SelectList(db.PieceType, "id", "name", sequence.PieceTypeId);
            ViewBag.remote_db_id = new SelectList(db.RemoteDb, "id", "name", sequence.RemoteDbId);
            return View(sequence);
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

            CommonSequence sequence = db.CommonSequence.Find(id);
            if (sequence == null)
            {
                return HttpNotFound();
            }

            return View(sequence);
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
            CommonSequence sequence = db.CommonSequence.Find(id);
            db.CommonSequence.Remove(sequence);
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
