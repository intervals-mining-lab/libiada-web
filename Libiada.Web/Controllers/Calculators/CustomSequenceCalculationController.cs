namespace Libiada.Web.Controllers.Calculators;

using System.Text.RegularExpressions;

using Bio;
using Bio.Extensions;

using Libiada.Core.Core;
using Libiada.Core.Core.SimpleTypes;
using Libiada.Core.Images;

using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Tasks;
using Libiada.Database.Helpers;
using Libiada.Database.Models.CalculatorsData;

using Newtonsoft.Json;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

/// <summary>
/// The quick calculation controller.
/// </summary>
[Authorize]
public class CustomSequenceCalculationController : AbstractResultController
{
    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomSequenceCalculationController"/> class.
    /// </summary>
    public CustomSequenceCalculationController(IViewDataHelper viewDataHelper, 
                                               ITaskManager taskManager,
                                               IFullCharacteristicRepository characteristicTypeLinkRepository) 
        : base(TaskType.CustomSequenceCalculation, taskManager)
    {
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.viewDataHelper = viewDataHelper;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        var imageTransformers = Extensions.EnumExtensions.GetSelectList<ImageTransformer>();

        Dictionary<string, object> viewData = viewDataHelper.GetCharacteristicsData(CharacteristicCategory.Full);
        viewData.Add("imageTransformers", imageTransformers);
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="characteristicLinkIds">
    /// The characteristic type link ids.
    /// </param>
    /// <param name="customSequences">
    /// The custom sequences.
    /// </param>
    /// <param name="localFile">
    /// The local file.
    /// </param>
    /// <param name="fileType">
    /// Uploaded files type.
    /// </param>
    /// <param name="toLower">
    /// Flag indicating that texts should be converted to lower case.
    /// </param>
    /// <param name="removePunctuation">
    /// Flag indicating that punctuations marks should be excluded from texts.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(short[] characteristicLinkIds,
                              string[] customSequences,
                              bool localFile,
                              string fileType,
                              bool? toLower,
                              bool? removePunctuation,
                              char? delimiter,
                              IFormFileCollection files)
    {
        return CreateTask(() =>
            {
                int sequencesCount = localFile ? files.Count : customSequences.Length;
                var sequencesNames = new string[sequencesCount];
                var sequences = new Chain[sequencesCount];
                if (localFile)
                {
                    for (int i = 0; i < sequencesCount; i++)
                    {
                        using Stream sequenceStream = Helpers.FileHelper.GetFileStream(files[i]);
                        sequencesNames[i] = files[i].FileName;

                        switch (fileType)
                        {
                            case "literature":
                                throw new NotImplementedException();
                            case "text":
                                using (var sr = new StreamReader(sequenceStream))
                                {
                                    string stringTextSequence = sr.ReadToEnd();
                                    if ((bool)toLower) stringTextSequence = stringTextSequence.ToLower();
                                    if ((bool)removePunctuation) stringTextSequence = Regex.Replace(stringTextSequence, @"[^\w\s]", "");
                                    sequences[i] = new Chain(stringTextSequence);
                                }
                                break;
                            case "image":
                                var image = Image.Load<Rgba32>(sequenceStream);
                                var sequence = ImageProcessor.ProcessImage(image, new IImageTransformer[0], new IMatrixTransformer[0], new LineOrderExtractor());
                                var alphabet = new Alphabet { NullValue.Instance() };
                                var incompleteAlphabet = sequence.Alphabet;
                                for (int j = 0; j < incompleteAlphabet.Cardinality; j++)
                                {
                                    alphabet.Add(incompleteAlphabet[j]);
                                }

                                sequences[i] = new Chain(sequence.Building, alphabet);
                                break;
                            case "genetic":
                                ISequence fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                                var stringSequence = fastaSequence.ConvertToString();
                                sequences[i] = new Chain(stringSequence);
                                sequencesNames[i] = fastaSequence.ID;
                                break;
                            case "wavFile":
                                var reader = new BinaryReader(Helpers.FileHelper.GetFileStream(files[i]));

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
                                byte[] byteArray = reader.ReadBytes(dataSize);
                                var shortArray = new short[byteArray.Length / 2];
                                Buffer.BlockCopy(byteArray, 0, shortArray, 0, byteArray.Length);
                                //shortArray = Amplitude(shortArray, 20);
                                shortArray = Sampling(shortArray, 50);
                                //shortArray = shortArray.Select(s => (short)(s / 10)).ToArray();
                                sequences[i] = new Chain(shortArray);
                                break;
                            default:
                                throw new ArgumentException("Unknown file type", nameof(fileType));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sequencesCount; i++)
                    {
                        sequences[i] = delimiter != null ? new Chain(customSequences[i].Split((char)delimiter).Select(el => (IBaseObject)new ValueString(el)).ToList()) : new Chain(customSequences[i]);
                        sequencesNames[i] = $"Custom sequence {i + 1}. Length: {customSequences[i].Length}";
                    }
                }

                var calculator = new CustomSequencesCharacterisitcsCalculator(characteristicTypeLinkRepository, characteristicLinkIds);
                var characteristics = calculator.Calculate(sequences).ToList();
                var sequencesCharacteristics = new List<SequenceCharacteristics>();
                for (int i = 0; i < sequences.Length; i++)
                {
                    sequencesCharacteristics.Add(new SequenceCharacteristics
                    {
                        MatterName = sequencesNames[i],
                        Characteristics = characteristics[i]
                    });
                }

                var characteristicNames = new string[characteristicLinkIds.Length];
                var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k]);
                    characteristicsList[k] = new SelectListItem
                    {
                        Value = k.ToString(),
                        Text = characteristicNames[k],
                        Selected = false
                    };
                }

                var result = new Dictionary<string, object>
                {
                    { "characteristics", sequencesCharacteristics },
                    { "characteristicNames", characteristicNames },
                    { "characteristicsList", characteristicsList }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
    }

    /// <summary>
    /// Cuts all amplitudes lower than given percent.
    /// </summary>
    /// <param name="shortArray">
    /// The initial amplitudes array.
    /// </param>
    /// <param name="percent">
    /// The percent.
    /// </param>
    /// <returns>
    /// The <see cref="T:short[]"/>.
    /// </returns>
    [NonAction]
    private short[] CutAmplitude(short[] shortArray, double percent)
    {
        short max = shortArray.Max(e => (short)System.Math.Max(e, -e));
        double threshold = max * percent / 100;
        return shortArray.Select(e => (e > threshold) || (e < -threshold) ? e : (short)0).ToArray();
    }

    [NonAction]
    private short[] Sampling(short[] shortArray, short cardinality)
    {
        short[] result = new short[shortArray.Length];
        short min = shortArray.Min();
        short max = shortArray.Max();
        var alphabet = new short[cardinality];
        for (int i = 0; i < cardinality; i++)
        {
            alphabet[i] = (short)((max - min) * i / cardinality + min);
        }

        for (int i = 0; i < shortArray.Length; i++)
        {
            short closest = 0;
            short difference = short.MaxValue;
            for (int j = 0; j < alphabet.Length; j++)
            {
                short currentDifference = (short)System.Math.Abs(alphabet[j] - shortArray[i]);
                if (difference > currentDifference)
                {
                    closest = alphabet[j];
                    difference = currentDifference;
                }
            }

            result[i] = closest;
        }

        return result;
    }
}
