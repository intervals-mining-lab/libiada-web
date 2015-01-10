namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The common sequences controller.
    /// </summary>
    public class CommonSequencesController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// The dna sequence repository.
        /// </summary>
        private readonly DnaSequenceRepository dnaSequenceRepository;

        /// <summary>
        /// The literature sequence repository.
        /// </summary>
        private readonly LiteratureSequenceRepository literatureSequenceRepository;

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
        /// Initializes a new instance of the <see cref="CommonSequencesController"/> class.
        /// </summary>
        public CommonSequencesController()
        {
            db = new LibiadaWebEntities();
            elementRepository = new ElementRepository(db);
            dnaSequenceRepository = new DnaSequenceRepository(db);
            literatureSequenceRepository = new LiteratureSequenceRepository(db);
            matterRepository = new MatterRepository(db);
            pieceTypeRepository = new PieceTypeRepository(db);
            notationRepository = new NotationRepository(db);
            remoteDbRepository = new RemoteDbRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            var commonSequence = db.CommonSequence.Include(c => c.Matter)
                                .Include(c => c.Notation)
                                .Include(c => c.PieceType)
                                .Include(c => c.RemoteDb);
            return View(await commonSequence.ToListAsync());
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CommonSequence commonSequence = await db.CommonSequence.FindAsync(id);
            if (commonSequence == null)
            {
                return HttpNotFound();
            }

            return View(commonSequence);
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
                    { "matters", matterRepository.GetMatterSelectList() }, 
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
        /// <param name="commonSequence">
        /// The common sequence.
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
        /// <param name="complementary">
        /// The complementary.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "Id,NotationId,MatterId,PieceTypeId,PiecePosition,RemoteDbId,RemoteId,Description")] CommonSequence commonSequence,
            bool localFile,
            int languageId,
            bool original,
            int? translatorId,
            int? productId,
            bool partial,
            bool complementary)
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
                        webApiId = NcbiHelper.GetId(commonSequence.RemoteId);
                        fileStream = NcbiHelper.GetFile(webApiId.ToString());
                    }

                    var input = new byte[fileStream.Length];

                    // Read the file into the byte array
                    fileStream.Read(input, 0, (int)fileStream.Length);
                    int natureId = db.Matter.Single(m => m.Id == commonSequence.MatterId).NatureId;

                    // Copy the byte array into a string
                    string stringSequence = natureId == Aliases.Nature.Genetic
                        ? Encoding.ASCII.GetString(input)
                        : Encoding.UTF8.GetString(input);

                    BaseChain chain;
                    long[] alphabet;
                    switch (natureId)
                    {
                        case Aliases.Nature.Genetic:

                            // отделяем заголовок fasta файла от цепочки
                            string[] splittedFasta = stringSequence.Split('\n', '\r');
                            var sequenceStringBuilder = new StringBuilder();
                            string fastaHeader = splittedFasta[0];
                            for (int j = 1; j < splittedFasta.Length; j++)
                            {
                                sequenceStringBuilder.Append(splittedFasta[j]);
                            }

                            string resultStringSequence = DataTransformers.CleanFastaFile(sequenceStringBuilder.ToString());

                            chain = new BaseChain(resultStringSequence);

                            if (!elementRepository.ElementsInDb(chain.Alphabet, commonSequence.NotationId))
                            {
                                throw new Exception("В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                            }

                            alphabet = elementRepository.ToDbElements(chain.Alphabet, commonSequence.NotationId, false);
                            dnaSequenceRepository.Insert(
                                commonSequence,
                                fastaHeader,
                                webApiId,
                                productId,
                                complementary,
                                partial,
                                alphabet,
                                chain.Building);
                            break;
                        case Aliases.Nature.Music:
                            break;
                        case Aliases.Nature.Literature:
                            string[] text = stringSequence.Split('\n');
                            for (int l = 0; l < text.Length - 1; l++)
                            {
                                // убираем \r
                                text[l] = text[l].Substring(0, text[l].Length - 1);
                            }

                            chain = new BaseChain(text.Length - 1);

                            // в конце файла всегда пустая строка поэтому последний элемент не считаем
                            // TODO: переделать этот говнокод и вообще добавить проверку на пустую строку в конце а лучше сделать нормальный trim
                            for (int i = 0; i < text.Length - 1; i++)
                            {
                                chain.Set(new ValueString(text[i]), i);
                            }

                            alphabet = elementRepository.ToDbElements(chain.Alphabet, commonSequence.NotationId, true);

                            literatureSequenceRepository.Insert(
                                commonSequence,
                                original,
                                languageId,
                                translatorId,
                                alphabet,
                                chain.Building);
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
                { "matters", matterRepository.GetMatterSelectList(commonSequence.MatterId) }, 
                { "notations", notationRepository.GetSelectListWithNature(commonSequence.NotationId) }, 
                { "languages", new SelectList(db.Language, "id", "name", languageId) }, 
                { "pieceTypes", pieceTypeRepository.GetSelectListWithNature(commonSequence.PieceTypeId) }, 
                { "remoteDbs", commonSequence.RemoteDbId == null
                        ? remoteDbRepository.GetSelectListWithNature()
                        : remoteDbRepository.GetSelectListWithNature((int)commonSequence.RemoteDbId) }, 
                { "natures", new SelectList(db.Nature, "id", "name", db.Matter.Single(m => m.Id == commonSequence.MatterId).NatureId) }, 
                { "natureLiterature", Aliases.Nature.Literature },
                { "natureGenetic", Aliases.Nature.Genetic },
                    { "translators", translators }
            };
            return View(commonSequence);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CommonSequence commonSequence = await db.CommonSequence.FindAsync(id);
            if (commonSequence == null)
            {
                return HttpNotFound();
            }

            ViewBag.MatterId = new SelectList(db.Matter, "Id", "Name", commonSequence.MatterId);
            ViewBag.NotationId = new SelectList(db.Notation, "Id", "Name", commonSequence.NotationId);
            ViewBag.PieceTypeId = new SelectList(db.PieceType, "Id", "Name", commonSequence.PieceTypeId);
            ViewBag.RemoteDbId = new SelectList(db.RemoteDb, "Id", "Name", commonSequence.RemoteDbId);
            return View(commonSequence);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,NotationId,MatterId,PieceTypeId,PiecePosition,RemoteDbId,RemoteId,Description")] CommonSequence commonSequence)
        {
            if (ModelState.IsValid)
            {
                db.Entry(commonSequence).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.MatterId = new SelectList(db.Matter, "Id", "Name", commonSequence.MatterId);
            ViewBag.NotationId = new SelectList(db.Notation, "Id", "Name", commonSequence.NotationId);
            ViewBag.PieceTypeId = new SelectList(db.PieceType, "Id", "Name", commonSequence.PieceTypeId);
            ViewBag.RemoteDbId = new SelectList(db.RemoteDb, "Id", "Name", commonSequence.RemoteDbId);
            return View(commonSequence);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CommonSequence commonSequence = await db.CommonSequence.FindAsync(id);
            if (commonSequence == null)
            {
                return HttpNotFound();
            }

            return View(commonSequence);
        }

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            CommonSequence commonSequence = await db.CommonSequence.FindAsync(id);
            db.CommonSequence.Remove(commonSequence);
            await db.SaveChangesAsync();
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
