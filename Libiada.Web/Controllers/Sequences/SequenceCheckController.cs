namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Core;
using Libiada.Database.Helpers;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;
using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Newtonsoft.Json;

using System.Text;

/// <summary>
/// The sequence check controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequenceCheckController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly IResearchObjectsCache cache;
    private readonly IViewDataBuilder viewDataBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceCheckController"/> class.
    /// </summary>
    public SequenceCheckController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                   ITaskManager taskManager,
                                   ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                   IResearchObjectsCache cache,
                                   IViewDataBuilder viewDataBuilder)
        : base(TaskType.SequenceCheck, taskManager)
    {
        this.dbFactory = dbFactory;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.cache = cache;
        this.viewDataBuilder = viewDataBuilder;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        var viewData = viewDataBuilder.SetNature(Nature.Genetic)
                                      .AddMinMaxResearchObjects(1, 1)
                                      .AddSequenceTypes(onlyGenetic: true)
                                      .AddGroups(onlyGenetic: true)
                                      .Build();
        ViewBag.data = JsonConvert.SerializeObject(viewData);
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
        Stream fileStream = Helpers.FileHelper.GetFileStream(file);

        return CreateTask(() =>
        {
            try
            {
                byte[] input = new byte[fileStream.Length];
                // Read the file into the byte array.
                fileStream.Read(input, 0, (int)fileStream.Length);

                // Copy the byte array into a string.
                string stringSequence = Encoding.ASCII.GetString(input);
                string[] tempString = stringSequence.Split('\n', '\r');
                string externalSequenceName = tempString[0];

                StringBuilder sequenceStringBuilder = new(stringSequence.Length);
                for (int j = 1; j < tempString.Length; j++)
                {
                    sequenceStringBuilder.Append(tempString[j]);
                }

                string resultStringSequence = DataTransformers.CleanFastaFile(sequenceStringBuilder.ToString());
                var sequence = new Sequence(resultStringSequence);

                CheckResult result;
                using var db = dbFactory.CreateDbContext();
                long sequenceId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == researchObjectId && c.Notation == Notation.Nucleotides).Id;
                using var sequenceRepository = sequenceRepositoryFactory.Create();
                Sequence dbSequence = sequenceRepository.GetLibiadaSequence(sequenceId);

                string dbSequenceName = db.ResearchObjects.Single(ro => ro.Id == researchObjectId).Name;
                // comparing sequences
                if (dbSequence.Equals(sequence))
                {
                    result = new CheckResult(
                        dbSequenceName,
                        externalSequenceName,
                        "Sequence in db and in file are equal",
                        "Success");
                }
                else
                {
                    // if they are not equal, comparing alphabets
                    if (sequence.Alphabet.Cardinality != dbSequence.Alphabet.Cardinality)
                    {
                        result = new CheckResult(
                            dbSequenceName,
                            externalSequenceName,
                            $"Alphabet sizes are not equal. In db - {dbSequence.Alphabet.Cardinality}. In file - {sequence.Alphabet.Cardinality}");

                        return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
                    }

                    for (int i = 0; i < sequence.Alphabet.Cardinality; i++)
                    {
                        if (!sequence.Alphabet[i].ToString().Equals(dbSequence.Alphabet[i].ToString()))
                        {
                            result = new CheckResult(
                                dbSequenceName,
                                externalSequenceName,
                                $"{i} elements in alphabet are not equal. In db - {dbSequence.Alphabet[i]}. In file - {sequence.Alphabet[i]}");
                            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
                        }
                    }

                    // if alphabets are equal, comparing orders
                    if (sequence.Length != dbSequence.Length)
                    {
                        result = new CheckResult(
                            dbSequenceName,
                            externalSequenceName,
                            $"Sequence length in db {dbSequence.Length}, and sequence length from file {sequence.Length}");
                        return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
                    }

                    int[] libiadaOrder = sequence.Order;
                    int[] databaseOrder = dbSequence.Order;

                    for (int j = 0; j < sequence.Length; j++)
                    {
                        if (libiadaOrder[j] != databaseOrder[j])
                        {
                            result = new CheckResult(
                                dbSequenceName,
                                externalSequenceName,
                                $"{j} sequences elements are not equal. In db {databaseOrder[j]}. In file {libiadaOrder[j]}");
                            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
                        }
                    }

                    result = new CheckResult(
                        dbSequenceName,
                        externalSequenceName,
                        "Sequences are equal and not equal at the same time.");
                }

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            }
            finally
            {
                fileStream.Dispose();
            }
        });
    }

    private record struct CheckResult(string dbSequenceName, string fileSequenceName, string message, string status = "Error");
}
