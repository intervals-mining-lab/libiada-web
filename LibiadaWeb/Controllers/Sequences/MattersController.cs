namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Xml;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The matters controller.
    /// </summary>
    public class MattersController : Controller
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
        /// Initializes a new instance of the <see cref="MattersController"/> class.
        /// </summary>
        public MattersController()
        {
            db = new LibiadaWebEntities();
            elementRepository = new ElementRepository(db);
            dnaSequenceRepository = new DnaSequenceRepository(db);
            literatureSequenceRepository = new LiteratureSequenceRepository(db);
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
            var matter = db.Matter.Include(m => m.Nature);
            return View(await matter.ToListAsync());
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

            Matter matter = await db.Matter.FindAsync(id);
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

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "notations", notationRepository.GetSelectListWithNature() },
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature() },
                    { "languages", new SelectList(db.Language, "id", "name") },
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
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="pieceTypeId">
        /// The piece type id.
        /// </param>
        /// <param name="remoteDbId">
        /// The remote db id.
        /// </param>
        /// <param name="remoteId">
        /// The remote id.
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
            [Bind(Include = "Id,NotationId,PieceTypeId,PiecePosition,RemoteDbId,RemoteId,Description,Matter")] CommonSequence commonSequence,
            bool localFile,
            int? languageId,
            bool? original,
            int? translatorId,
            int? productId,
            bool? partial,
            bool? complementary)
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
                            throw new ArgumentNullException("file", "Sequence file is null or empty");
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

                    string stringSequence = commonSequence.Matter.NatureId == Aliases.Nature.Genetic
                        ? Encoding.ASCII.GetString(input)
                        : Encoding.UTF8.GetString(input);

                    BaseChain chain;
                    long[] alphabet;

                    switch (commonSequence.Matter.NatureId)
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
                                throw new Exception("At least one element of new sequence missing in db.");
                            }

                            commonSequence.Matter.Sequence = new Collection<CommonSequence>();
                            
                            db.Matter.Add(commonSequence.Matter);
                            db.SaveChanges();

                            commonSequence.MatterId = commonSequence.Matter.Id;
                            matterCreated = true;

                            alphabet = elementRepository.ToDbElements(chain.Alphabet, commonSequence.NotationId, false);
                            dnaSequenceRepository.Insert(commonSequence, fastaHeader, webApiId, productId, complementary ?? false, partial ?? false, alphabet, chain.Building);
                            break;
                        case Aliases.Nature.Music:
                            var doc = new XmlDocument();
                            doc.LoadXml(stringSequence);
                            
                            // MusicXmlParser parser = new MusicXmlParser();
                            // parser.Execute(doc, "test");
                            // ScoreTrack tempTrack = parser.ScoreModel;
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
                            for (int i = 0; i < text.Length - 1; i++)
                            {
                                chain.Set(new ValueString(text[i]), i);
                            }

                            commonSequence.Matter.Sequence = new Collection<CommonSequence>();

                            db.Matter.Add(commonSequence.Matter);
                            db.SaveChanges();

                            commonSequence.MatterId = commonSequence.Matter.Id;

                            alphabet = elementRepository.ToDbElements(
                                chain.Alphabet,
                                commonSequence.NotationId,
                                true);

                            literatureSequenceRepository.Insert(
                                commonSequence,
                                original ?? false,
                                languageId ?? 0,
                                translatorId,
                                alphabet,
                                chain.Building);

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
                        { "notations", notationRepository.GetSelectListWithNature(commonSequence.NotationId) },
                        { "pieceTypes", pieceTypeRepository.GetSelectListWithNature(commonSequence.PieceTypeId) },
                        { "languages", new SelectList(db.Language, "id", "name", languageId) },
                        { "remoteDbs", commonSequence.RemoteDbId.HasValue ? 
                            remoteDbRepository.GetSelectListWithNature(commonSequence.RemoteDbId.Value) :
                            remoteDbRepository.GetSelectListWithNature() },
                        { "natures", new SelectList(db.Nature, "id", "name", commonSequence.Matter.NatureId) },
                        { "translators", new SelectList(db.Translator, "id", "name") },
                        { "natureLiterature", Aliases.Nature.Literature },
                        { "natureGenetic", Aliases.Nature.Genetic }
                    };
            return View(commonSequence.Matter);
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

            Matter matter = await db.Matter.FindAsync(id);
            if (matter == null)
            {
                return HttpNotFound();
            }

            ViewBag.NatureId = new SelectList(db.Nature, "Id", "Name", matter.NatureId);
            return View(matter);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,NatureId,Description")] Matter matter)
        {
            if (ModelState.IsValid)
            {
                db.Entry(matter).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.NatureId = new SelectList(db.Nature, "Id", "Name", matter.NatureId);
            return View(matter);
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

            Matter matter = await db.Matter.FindAsync(id);
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
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            Matter matter = await db.Matter.FindAsync(id);
            db.Matter.Remove(matter);
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
