namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Xml;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The sequences matters controller.
    /// </summary>
    public abstract class SequencesMattersController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        protected readonly LibiadaWebEntities Db;

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
        /// The data sequence repository.
        /// </summary>
        private readonly DataSequenceRepository dataSequenceRepository;

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
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesMattersController"/> class.
        /// </summary>
        protected SequencesMattersController()
        {
            Db = new LibiadaWebEntities();
            elementRepository = new ElementRepository(Db);
            dnaSequenceRepository = new DnaSequenceRepository(Db);
            literatureSequenceRepository = new LiteratureSequenceRepository(Db);
            dataSequenceRepository = new DataSequenceRepository(Db);
            pieceTypeRepository = new PieceTypeRepository(Db);
            notationRepository = new NotationRepository(Db);
            remoteDbRepository = new RemoteDbRepository(Db);
            matterRepository = new MatterRepository(Db);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.dbName = DbHelper.GetDbName(Db);

            var translators = new SelectList(Db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature() }, 
                    { "languages", new SelectList(Db.Language, "id", "name") }, 
                    { "remoteDbs", remoteDbRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(Db.Nature, "id", "name") }, 
                    { "translators", translators }, 
                    { "natureLiterature", Aliases.Nature.Literature }
                };
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="commonSequence">
        /// The sequence.
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
        /// <exception cref="Exception">
        /// Thrown if nature id of notation of sequence is unknown. 
        /// </exception>
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
                try
                {
                int natureId = Db.Notation.Single(m => m.Id == commonSequence.NotationId).NatureId;
                int? webApiId = null;

                string stringSequence;

                if (localFile)
                {
                    var encoding = natureId == Aliases.Nature.Genetic ? Encoding.ASCII : Encoding.UTF8;
                    stringSequence = FileHelper.ReadFileStream(Request.Files[0], encoding);
                }
                else
                {
                    webApiId = NcbiHelper.GetId(commonSequence.RemoteId);
                    stringSequence = NcbiHelper.GetSequenceString(webApiId.ToString());
                }

                switch (natureId)
                {
                    case Aliases.Nature.Genetic:
                        CreateDnaSequence(commonSequence, productId, partial ?? false, complementary ?? false, stringSequence, webApiId);
                        break;
                    case Aliases.Nature.Music:
                        var doc = new XmlDocument();
                        doc.LoadXml(stringSequence);

                        // MusicXmlParser parser = new MusicXmlParser();
                        // parser.Execute(doc, "test");
                        // ScoreTrack tempTrack = parser.ScoreModel;
                        break;
                    case Aliases.Nature.Literature:
                        CreateLiteratureSequence(commonSequence, languageId ?? 0, original ?? false, translatorId, stringSequence);
                        break;
                    case Aliases.Nature.Data:
                        CreateDataSequence(commonSequence, stringSequence);
                        break;
                    default:
                        throw new Exception("Unknown nature.");
                }

                return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("Error", e.Message);
                }
            }

            var translators = new SelectList(Db.Translator, "id", "name").ToList();
            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
            {
                    { "matters", matterRepository.GetMatterSelectList(commonSequence.MatterId) }, 
                    { "notations", notationRepository.GetSelectListWithNature(commonSequence.NotationId) }, 
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature(commonSequence.PieceTypeId) }, 
                    { "languages", new SelectList(Db.Language, "id", "name", languageId) }, 
                    { "remoteDbs", commonSequence.RemoteDbId.HasValue ? 
                        remoteDbRepository.GetSelectListWithNature(commonSequence.RemoteDbId.Value) :
                        remoteDbRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(Db.Nature, "id", "name", Db.Notation.Single(m => m.Id == commonSequence.NotationId).NatureId) }, 
                    { "translators", translators }, 
                    { "natureLiterature", Aliases.Nature.Literature }
            };
            return View(commonSequence);
        }

        /// <summary>
        /// The create data sequence.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        /// <param name="stringSequence">
        /// The string sequence.
        /// </param>
        private void CreateDataSequence(CommonSequence commonSequence, string stringSequence)
        {
            string[] text = stringSequence.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var sequence = text.Where(t => !t.Equals("\"volume\"") && !string.IsNullOrEmpty(t) && !string.IsNullOrWhiteSpace(t)).ToList();

            var elements = new List<IBaseObject>();

            for (int i = 0; i < sequence.Count; i++)
            {
                elements.Add(new ValueInt(int.Parse(sequence[i].Substring(0, sequence[i].Length - 2))));
            }

            var chain = new BaseChain(elements);

            matterRepository.CreateMatterFromSequence(commonSequence);

            var alphabet = elementRepository.ToDbElements(chain.Alphabet, commonSequence.NotationId, true);
            dataSequenceRepository.Insert(commonSequence, alphabet, chain.Building);
        }

        /// <summary>
        /// The create literature sequence.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
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
        /// <param name="stringSequence">
        /// The string sequence.
        /// </param>
        private void CreateLiteratureSequence(CommonSequence commonSequence, int languageId, bool original, int? translatorId, string stringSequence)
        {
            string[] text = stringSequence.Split('\n');
            for (int l = 0; l < text.Length - 1; l++)
            {
                // убираем \r
                text[l] = text[l].Substring(0, text[l].Length - 1);
            }

            var chain = new BaseChain(text.Length - 1);

            // в конце файла всегда пустая строка поэтому последний элемент не считаем
            // TODO: переделать этот говнокод и вообще добавить проверку на пустую строку в конце, а лучше сделать нормальный trim
            for (int i = 0; i < text.Length - 1; i++)
            {
                chain.Set(new ValueString(text[i]), i);
            }

            matterRepository.CreateMatterFromSequence(commonSequence);

            var alphabet = elementRepository.ToDbElements(chain.Alphabet, commonSequence.NotationId, true);
            literatureSequenceRepository.Insert(commonSequence, original, languageId, translatorId, alphabet, chain.Building);
        }

        /// <summary>
        /// The create dna sequence.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
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
        /// <param name="stringSequence">
        /// The string sequence.
        /// </param>
        /// <param name="webApiId">
        /// The web api id.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if at least one element of new sequence is missing in db.
        /// </exception>
        private void CreateDnaSequence(CommonSequence commonSequence, int? productId, bool partial, bool complementary, string stringSequence, int? webApiId)
        {
            // отделяем заголовок fasta файла от цепочки
            string[] splittedFasta = stringSequence.Split('\n', '\r');
            var sequenceStringBuilder = new StringBuilder();
            string fastaHeader = splittedFasta[0];
            for (int j = 1; j < splittedFasta.Length; j++)
            {
                sequenceStringBuilder.Append(splittedFasta[j]);
            }

            string resultStringSequence = DataTransformers.CleanFastaFile(sequenceStringBuilder.ToString());

            var chain = new BaseChain(resultStringSequence);

            if (!elementRepository.ElementsInDb(chain.Alphabet, commonSequence.NotationId))
            {
                throw new Exception("At least one element of new sequence is missing in db.");
            }

            matterRepository.CreateMatterFromSequence(commonSequence);

            var alphabet = elementRepository.ToDbElements(chain.Alphabet, commonSequence.NotationId, false);
            dnaSequenceRepository.Insert(commonSequence, fastaHeader, webApiId, productId, complementary, partial, alphabet, chain.Building);
        }
    }
}
