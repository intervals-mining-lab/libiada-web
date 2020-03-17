namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaCore.Images;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using SixLabors.ImageSharp;

    /// <summary>
    /// The quick calculation controller.
    /// </summary>
    [Authorize]
    public class CustomSequenceCalculationController : AbstractResultController
    {
        /// <summary>
        /// The characteristic type link repository.
        /// </summary>
        private readonly FullCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceCalculationController"/> class.
        /// </summary>
        public CustomSequenceCalculationController() : base(TaskType.CustomSequenceCalculation)
        {
            characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var imageTransformers = EnumHelper.GetSelectList(typeof(ImageTransformer));

            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                Dictionary<string, object> viewData = viewDataHelper.GetCharacteristicsData(CharacteristicCategory.Full);
                viewData.Add("imageTransformers", imageTransformers);
                ViewBag.data = JsonConvert.SerializeObject(viewData);
                return View();
            }
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
        public ActionResult Index(short[] characteristicLinkIds, string[] customSequences, bool localFile, string fileType, bool toLower, bool removePunctuation)
        {
            return CreateTask(() =>
                {
                    int sequencesCount = localFile ? Request.Files.Count : customSequences.Length;
                    var sequencesNames = new string[sequencesCount];
                    var sequences = new Chain[sequencesCount];
                    if (localFile)
                    {
                        for (int i = 0; i < sequencesCount; i++)
                        {

                            Stream sequenceStream = FileHelper.GetFileStream(Request.Files[i]);
                            sequencesNames[i] = Request.Files[i].FileName;

                            switch (fileType)
                            {
                                case "text":
                                    using (var sr = new StreamReader(sequenceStream))
                                    {
                                        string stringTextSequence = sr.ReadToEnd();
                                        if (toLower) stringTextSequence = stringTextSequence.ToLower();
                                        if (removePunctuation) stringTextSequence = Regex.Replace(stringTextSequence, @"[^\w\s]", "");
                                        sequences[i] = new Chain(stringTextSequence);
                                    }
                                    break;
                                case "image":
                                    var image = Image.Load(sequenceStream);
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
                                    var reader = new BinaryReader(Request.Files[i].InputStream);

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
                            sequences[i] = new Chain(customSequences[i]);
                            sequencesNames[i] = $"Custom sequence {i + 1}. Length: {customSequences[i].Length}";
                        }
                    }

                    var sequencesCharacteristics = new SequenceCharacteristics[sequences.Length];
                    for (int j = 0; j < sequences.Length; j++)
                    {
                        var characteristics = new double[characteristicLinkIds.Length];
                        for (int k = 0; k < characteristicLinkIds.Length; k++)
                        {
                            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkIds[k]);
                            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkIds[k]);
                            IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);

                            characteristics[k] = calculator.Calculate(sequences[j], link);
                        }

                        sequencesCharacteristics[j] = new SequenceCharacteristics
                        {
                            MatterName = sequencesNames[j],
                            Characteristics = characteristics
                        };
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

                    return new Dictionary<string, object>
                               {
                                   { "data", JsonConvert.SerializeObject(result) }
                               };
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
        private short[] CutAmplitude(short[] shortArray, double percent)
        {
            short max = shortArray.Max(e => (short)Math.Max(e, -e));
            double threshold = max * percent / 100;
            return shortArray.Select(e => (e > threshold) || (e < -threshold) ? e : (short)0).ToArray();
        }

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
                    short currentDifference = (short)Math.Abs(alphabet[j] - shortArray[i]);
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
}
