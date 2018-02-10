namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaCore.Images;

    using LibiadaWeb.Helpers;
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
            var fullCharacteristicRepository = FullCharacteristicRepository.Instance;
            ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "characteristicTypes", fullCharacteristicRepository.GetCharacteristicTypes() }
                });

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
        /// <param name="file">
        /// The files.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int[] characteristicLinkIds, string[] customSequences, bool localFile, string fileType, HttpPostedFileBase[] file)
        {
            return CreateTask(() =>
                {
                    int sequencesCount = localFile ? Request.Files.Count : customSequences.Length;
                    var names = new string[sequencesCount];
                    var sequences = new Chain[sequencesCount];

                    for (int i = 0; i < sequencesCount; i++)
                    {
                        if (localFile)
                        {
                            Stream sequenceStream = FileHelper.GetFileStream(file[i]);
                            switch (fileType)
                            {
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
                                    names[i] = Request.Files[i].FileName;
                                    break;
                                case "genetic":
                                    ISequence fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                                    var stringSequence = fastaSequence.ConvertToString();
                                    sequences[i] = new Chain(stringSequence);
                                    names[i] = fastaSequence.ID;
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
                                    names[i] = Request.Files[i].FileName;
                                    byte[] byteArray = reader.ReadBytes(dataSize);
                                    short[] shortArray = new short[byteArray.Length / 2];
                                    Buffer.BlockCopy(byteArray, 0, shortArray, 0, byteArray.Length);
                                    sequences[i] = new Chain(shortArray);
                                    break;
                                default:
                                    throw new ArgumentException("Unknown file type", nameof(fileType));
                            }
                        }
                        else
                        {
                            sequences[i] = new Chain(customSequences[i]);
                            names[i] = $"Custom sequence {i + 1}. Length: {customSequences[i].Length}";
                        }
                    }

                    var characteristics = new double[sequences.Length, characteristicLinkIds.Length];

                    for (int j = 0; j < sequences.Length; j++)
                    {
                        for (int k = 0; k < characteristicLinkIds.Length; k++)
                        {
                            sequences[j].FillIntervalManagers();

                            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkIds[k]);
                            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkIds[k]);
                            IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);

                            characteristics[j, k] = calculator.Calculate(sequences[j], link);
                        }
                    }

                    List<string> characteristicNames = characteristicLinkIds.ConvertAll(c => characteristicTypeLinkRepository.GetCharacteristicName(c));

                    return new Dictionary<string, object>
                    {
                        { "names", names },
                        { "characteristicNames", characteristicNames },
                        { "characteristics", characteristics }
                    };
                });
        }
    }
}
