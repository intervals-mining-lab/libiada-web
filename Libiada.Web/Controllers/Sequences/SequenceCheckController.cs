namespace Libiada.Web.Controllers.Sequences;

using System.Text;

using Libiada.Core.Core;

using Libiada.Database.Helpers;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;




/// <summary>
/// The sequence check controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequenceCheckController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly ResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceCheckController"/> class.
    /// </summary>
    public SequenceCheckController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                   ITaskManager taskManager,
                                   ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                   ResearchObjectsCache cache)
        : base(TaskType.SequenceCheck, taskManager)
    {
        this.dbFactory = dbFactory;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.cache = cache;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        ViewBag.researchObjectId = new SelectList(cache.ResearchObjects.Where(m => m.Nature == Nature.Genetic).ToArray(), "id", "name");
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectId">
    /// The research object id.
    /// </param>
    /// <param name="file">
    /// The file.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(long researchObjectId, IFormFile file)
    {
        return CreateTask(() =>
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException(nameof(file), "Sequence file not found or empty.");
            }

            // Initialize the stream.
            using Stream fileStream = Helpers.FileHelper.GetFileStream(file);
            byte[] input = new byte[fileStream.Length];
            // Read the file into the byte array.
            fileStream.Read(input, 0, (int)fileStream.Length);

            // Copy the byte array into a string.
            string stringSequence = Encoding.ASCII.GetString(input);
            string[] tempString = stringSequence.Split('\n', '\r');

            var sequenceStringBuilder = new StringBuilder(stringSequence.Length);

            for (int j = 1; j < tempString.Length; j++)
            {
                sequenceStringBuilder.Append(tempString[j]);
            }

            string resultStringSequence = DataTransformers.CleanFastaFile(sequenceStringBuilder.ToString());

            var sequence = new Sequence(resultStringSequence);
            string message;
            string status;
            Sequence dbSequence;
            using var db = dbFactory.CreateDbContext();
            long sequenceId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == researchObjectId).Id;
            using var sequenceRepository = sequenceRepositoryFactory.Create();
            dbSequence = sequenceRepository.GetLibiadaSequence(sequenceId);


            if (dbSequence.Equals(sequence))
            {
                message = "Sequence in db and in file are equal";
                status = "Success";
            }
            else
            {
                status = "Error";
                if (sequence.Alphabet.Cardinality != dbSequence.Alphabet.Cardinality)
                {
                    message = $"Alphabet sizes are not equal. In db - {dbSequence.Alphabet.Cardinality}. In file - {sequence.Alphabet.Cardinality}";
                    return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message }) } };
                }

                for (int i = 0; i < sequence.Alphabet.Cardinality; i++)
                {
                    if (!sequence.Alphabet[i].ToString().Equals(dbSequence.Alphabet[i].ToString()))
                    {
                        message = $"{i} elements in alphabet are not equal. In db - {dbSequence.Alphabet[i]}. In file - {sequence.Alphabet[i]}";
                        return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message }) } };
                    }
                }

                if (sequence.Length != dbSequence.Length)
                {
                    message = $"Sequence length in db {dbSequence.Length}, and sequence length from file{sequence.Length}";
                    return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
                }

                int[] libiadaOrder = sequence.Order;
                int[] databaseOrder = dbSequence.Order;

                for (int j = 0; j < sequence.Length; j++)
                {
                    if (libiadaOrder[j] != databaseOrder[j])
                    {
                        message = $"{j} sequences elements are not equal. In db {databaseOrder[j]}. In file {libiadaOrder[j]}";
                        return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
                    }
                }

                message = "Sequences are equal and not equal at the same time.";
            }

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
        });
    }
}
