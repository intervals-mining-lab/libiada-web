namespace Libiada.Web.Controllers.Calculators;

using System.Globalization;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Core.Extensions;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

/// <summary>
/// The sequence prediction controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequencePredictionController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequencePredictionController"/> class.
    /// </summary>
    public SequencePredictionController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                        IViewDataHelper viewDataHelper, 
                                        ITaskManager taskManager,
                                        IFullCharacteristicRepository characteristicTypeLinkRepository,
                                        Cache cache)
        : base(TaskType.SequencePrediction, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
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
        var viewData = viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, 1, "Predict");
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="matterId">
    /// The matter id.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic link id.
    /// </param>
    /// <param name="notation">
    /// The notation.
    /// </param>
    /// <param name="step">
    /// The step.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(
        long matterId,
        short characteristicLinkId,
        Notation notation,
        int step,
        int initialLength,
        string accuracy)
    {
        return CreateTask(() =>
        {
            string characteristicName;
            string matterName;
            double[] characteristics;
            Chain sequence;
            IFullCalculator calculator;
            Link link;

            var commonSequenceRepository = new CommonSequenceRepository(dbFactory, cache);
            matterName = cache.Matters.Single(m => matterId == m.Id).Name;
            using var db = dbFactory.CreateDbContext();
            var sequenceId = db.CommonSequences.Single(c => matterId == c.MatterId && c.Notation == notation).Id;
            sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);

            characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);

            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
            link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

            // characteristics = SequencesCharacteristicsCalculator.Calculate( new[] { sequenceId }, characteristicLinkId);

            AverageRemoteness averageRemotenessCalc = new AverageRemoteness();
            double averageRemoteness = averageRemotenessCalc.Calculate(sequence, Link.Start);
            Alphabet alphabet = sequence.Alphabet;
            var doubleAccuracy = double.Parse(accuracy, CultureInfo.InvariantCulture);

            List<SequencePredictionData> sequencePredictionResult;
            Chain chain;
            (sequencePredictionResult, chain) = Predict(averageRemotenessCalc, sequence, initialLength, alphabet, averageRemoteness, doubleAccuracy);

            var matching = FindPercentageOfMatching(sequence, chain, initialLength) * 100;


            var result = new Dictionary<string, object>
            {
                { "result", sequencePredictionResult },
                { "matterName", matterName },
                { "matching", matching }
            };

            return new Dictionary<string, string>
            {
                { "data", JsonConvert.SerializeObject(result) }
            };
        });
    }

    //[NonAction]
    //private Chain IncrementNextCharacter(Chain target, int startElement, Alphabet alphabet)
    //{
    //    if (startElement + 1 < target.Length)
    //    {
    //        if (IsLast(target.Get(startElement + 1), alphabet))
    //        {
    //            IBaseObject firstLetter = alphabet.First();
    //            target.Set(firstLetter, startElement + 1);
    //            target = IncrementNextCharacter(target, startElement + 1, alphabet);
    //        }
    //        else
    //        {
    //            IBaseObject letter = GetElementAfter(target.Get(startElement + 1), alphabet);
    //            target.Set(letter, startElement + 1);
    //        }
    //    }
    //    else
    //    {
    //        target = ExtendAndCopy(target, 1);
    //        IBaseObject firstLetter = alphabet.First();
    //        target.Set(firstLetter, target.Length - 1);
    //    }
    //    return target;
    //}

    [NonAction]
    private IBaseObject GetElementAfter(IBaseObject element, Alphabet alphabet)
    {
        bool current = false;
        foreach (IBaseObject alphabetElement in alphabet)
        {
            if (current)
            {
                return alphabetElement;
            }
            if (alphabetElement.Equals(element))
            {
                current = true;
            }
        }

        throw new Exception();
    }

    [NonAction]
    private bool IsLast(IBaseObject letter, Alphabet alphabet)
    {
        return alphabet.Last().Equals(letter);
    }

    [NonAction]
    private Chain ExtendAndCopy(Chain source, int extensionLength)
    {
        return Copy(source, new Chain(source.Length + extensionLength));
    }

    [NonAction]
    private Chain Copy(Chain source, Chain destanation)
    {
        int commonLenth = System.Math.Min(source.Length, destanation.Length);
        for (int i = 0; i < commonLenth; i++)
        {
            destanation.Set(source.Get(i), i);
        }
        return destanation;
    }

    [NonAction]
    private Chain Concat(Chain left, Chain right, int indexStart)
    {
        var result = new List<IBaseObject>(indexStart + right.Length);
        result.AddRange(left.ToArray().SubArray(0, indexStart));
        result.AddRange(right.ToArray());
        return new Chain(result);
    }

    [NonAction]
    private (List<SequencePredictionData>, Chain) Predict(
        AverageRemoteness averageRemotenessCalc,
        Chain sequence,
        int initialLength,
        Alphabet alphabet,
        double averageRemoteness,
        double accuracy)
    {
        var sequencePredictionResult = new List<SequencePredictionData>();

        int wordPositionStart = initialLength;
        Chain currentPredicion = null;
        Chain predicted = Copy(sequence, new Chain(initialLength));
        for (int i = initialLength; i < sequence.Length; i++)
        {
            currentPredicion = Copy(predicted, new Chain(i + 1));
            Dictionary<double, ContenderValue> contenderValues = new Dictionary<double, ContenderValue>();
            bool isFound = false;
            foreach (IBaseObject element in alphabet)
            {
                currentPredicion.Set(element, wordPositionStart);
                double currentAvgRemoteness = averageRemotenessCalc.Calculate(currentPredicion, Link.Start);
                double delta = System.Math.Abs(currentAvgRemoteness - averageRemoteness);

                if (delta < accuracy)
                {
                    contenderValues.Add(delta, new ContenderValue
                    {
                        CurrentAverageRemoteness = currentAvgRemoteness,
                        PredictedWord = SubChain(currentPredicion, wordPositionStart, i)
                    });
                    isFound = true;
                }
            }

            if (isFound)
            {
                ContenderValue contenderValue = contenderValues[contenderValues.Keys.Min()];
                sequencePredictionResult.Add(new SequencePredictionData
                {
                    Fragment = SubChain(sequence, wordPositionStart, i).ToString(),
                    Predicted = contenderValue.PredictedWord.ToString(),
                    ActualCharacteristic = contenderValue.CurrentAverageRemoteness,
                    TheoreticalCharacteristic = averageRemotenessCalc.Calculate(SubChain(sequence, 0, i), Link.Start)
                    //PercentageOfMatched = FindPercentageOfMatching(sequence, currentPredicion)
                });

                predicted = Concat(predicted, contenderValue.PredictedWord, wordPositionStart);

                wordPositionStart = i + 1;
            }
            else
            {
                throw new Exception($"Couldn't predict with given accuracy. Position: {i}");
                //currentPredicion = IncrementNextCharacter(currentPredicion, wordPositionStart, alphabet);
            }
        }

        return (sequencePredictionResult, currentPredicion);
    }

    // todo fix throwing exception
    [NonAction]
    private Chain SubChain(Chain source, int start, int end)
    {
        Chain chain = new Chain(end - start + 1);
        for (int i = 0; i < chain.Length; i++)
        {
            chain.Set(source.Get(start + i), i);
        }
        return chain;
    }

    [NonAction]
    private double FindPercentageOfMatching(Chain first, Chain second, int startIndex)
    {
        int count = 0;
        for (int i = startIndex; i < second.Length; i++)
        {
            if (first.Get(i).Equals(second.Get(i)))
            {
                count++;
            }
        }

        return (double)count / (second.Length - startIndex);
    }

    private struct ContenderValue
    {
        public double CurrentAverageRemoteness;
        public Chain PredictedWord;
    }
}
