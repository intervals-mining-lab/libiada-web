namespace Libiada.Web.Controllers.Calculators;

using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;

/// <summary>
/// The music files controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class MusicFilesController : AbstractResultController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MusicFilesController"/> class.
    /// </summary>
    public MusicFilesController(ITaskManager taskManager) : base(TaskType.MusicFiles, taskManager)
    {
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="file">
    /// The file.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(List<IFormFile> files)
    {
        var fileStreams = files.Select(Helpers.FileHelper.GetFileStream).ToList();
        return CreateTask(() =>
        {
            string[] names = new string[files.Count];
            object[] data = new object[files.Count];

            for (int i = 0; i < files.Count; i++)
            {
                names[i] = files[i].FileName;
                using var reader = new BinaryReader(fileStreams[i]);

                int chunkID = reader.ReadInt32();
                int fileSize = reader.ReadInt32();
                int riffType = reader.ReadInt32();
                int fmtID = reader.ReadInt32();
                int fmtSize = reader.ReadInt32();
                int fmtCode = reader.ReadInt16();
                int channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int fmtAvgBPS = reader.ReadInt32();
                int fmtBlockAlign = reader.ReadInt16();
                int bitDepth = reader.ReadInt16();

                if (fmtSize == 18)
                {
                    // Read any extra values
                    int fmtExtraSize = reader.ReadInt16();
                    reader.ReadBytes(fmtExtraSize);
                }

                int dataID = reader.ReadInt32();
                int dataSize = reader.ReadInt32();

                data[i] = new { name = files[i].FileName, sampleRate, channels, audioFormat = fmtCode, sampleSize = fmtBlockAlign };

            }

            var result = new Dictionary<string, object>() { { "data", data } };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
