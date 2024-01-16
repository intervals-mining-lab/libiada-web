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
    private readonly ILibiadaDatabaseEntitiesFactory dbFactory;
    private readonly ICommonSequenceRepository commonSequenceRepository;
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceCheckController"/> class.
    /// </summary>
    public SequenceCheckController(ILibiadaDatabaseEntitiesFactory dbFactory, 
                                   ITaskManager taskManager, 
                                   ICommonSequenceRepository commonSequenceRepository, 
                                   Cache cache) 
        : base(TaskType.SequenceCheck, taskManager)
    {
        this.dbFactory = dbFactory;
        this.commonSequenceRepository = commonSequenceRepository;
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
        ViewBag.matterId = new SelectList(cache.Matters.Where(m => m.Nature == Nature.Genetic).ToArray(), "id", "name");
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="matterId">
    /// The matter id.
    /// </param>
    /// <param name="file">
    /// The file.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(long matterId, IFormFile file)
    {
        return CreateTask(() =>
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException(nameof(file), "Sequence file not found or empty.");
            }

            // Initialize the stream.
            using var fileStream = Helpers.FileHelper.GetFileStream(file);
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

            var chain = new BaseChain(resultStringSequence);
            string message;
            string status;
            BaseChain dbChain;
            using var db = dbFactory.CreateDbContext();
            long sequenceId = db.CommonSequences.Single(c => c.MatterId == matterId).Id;
            dbChain = commonSequenceRepository.GetLibiadaBaseChain(sequenceId);


            if (dbChain.Equals(chain))
            {
                message = "Sequence in db and in file are equal";
                status = "Success";
            }
            else
            {
                status = "Error";
                if (chain.Alphabet.Cardinality != dbChain.Alphabet.Cardinality)
                {
                    message = $"Alphabet sizes are not equal. In db - {dbChain.Alphabet.Cardinality}. In file - {chain.Alphabet.Cardinality}";
                    return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message }) } };
                }

                for (int i = 0; i < chain.Alphabet.Cardinality; i++)
                {
                    if (!chain.Alphabet[i].ToString().Equals(dbChain.Alphabet[i].ToString()))
                    {
                        message = $"{i} elements in alphabet are not equal. In db - {dbChain.Alphabet[i]}. In file - {chain.Alphabet[i]}";
                        return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message }) } };
                    }
                }

                if (chain.Length != dbChain.Length)
                {
                    message = $"Sequence length in db {dbChain.Length}, and sequence length from file{chain.Length}";
                    return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
                }

                int[] libiadaBuilding = chain.Building;
                int[] dataBaseBuilding = dbChain.Building;

                for (int j = 0; j < chain.Length; j++)
                {
                    if (libiadaBuilding[j] != dataBaseBuilding[j])
                    {
                        message = $"{j} sequences elements are not equal. In db {dataBaseBuilding[j]}. In file {libiadaBuilding[j]}";
                        return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
                    }
                }

                message = "Sequences are equal and not equal at the same time.";
            }

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(new { message, status }) } };
        });
    }
}
