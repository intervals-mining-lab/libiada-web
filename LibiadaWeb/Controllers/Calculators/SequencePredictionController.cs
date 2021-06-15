namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The sequence prediction controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequencePredictionController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePredictionController"/> class.
        /// </summary>
        public SequencePredictionController() : base(TaskType.SequencePrediction)
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
            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                var viewData = viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, 1, "Predict");
                ViewBag.data = JsonConvert.SerializeObject(viewData);
                return View();
            }
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
                string mattersName;
                double[] characteristics;
                Chain sequence;
                IFullCalculator calculator;
                Link link;

                using (var db = new LibiadaWebEntities())
                {
                    var commonSequenceRepository = new CommonSequenceRepository(db);
                    mattersName = Cache.GetInstance().Matters.Single(m => matterId == m.Id).Name;
                    var sequenceId = db.CommonSequence.Single(c => matterId == c.MatterId && c.Notation == notation).Id;
                    sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);

                    var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                    characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);

                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                    calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
                    link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                }

                // characteristics = SequencesCharacteristicsCalculator.Calculate( new[] { sequenceId }, characteristicLinkId);

                AverageRemoteness averageRemotenessCalc = new AverageRemoteness();
                double averageRemoteness = averageRemotenessCalc.Calculate(sequence, Link.Start);
                Alphabet alphabet = sequence.Alphabet;
                var doubleAccuracy = double.Parse(accuracy);

                List<SequencePredictionData> sequencePredictionResult;
                Chain chain;
                (sequencePredictionResult, chain) = Predict(averageRemotenessCalc, sequence, initialLength, alphabet, averageRemoteness, doubleAccuracy);

                var matching = FindPercentageOfMatching(sequence, chain) * 100;


                var result = new Dictionary<string, object>
                {
                    { "result", sequencePredictionResult },
                    {"matching", matching }
                };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(result) }
                };
            });
        }

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

        private bool IsLast(IBaseObject letter, Alphabet alphabet)
        {
            return alphabet.Last().Equals(letter);
        }

        private Chain ExtendAndCopy(Chain source, int extensionLength)
        {
            return Copy(source, new Chain(source.Length + extensionLength));
        }

        private Chain Copy(Chain source, Chain destanation)
        {
            int commonLenth = Math.Min(source.Length, destanation.Length);
            for (int i = 0; i < commonLenth; i++)
            {
                destanation.Set(source.Get(i), i);
            }
            return destanation;
        }

        private Chain Concat(Chain left, Chain right, int indexStart)
        {
            var result = new List<IBaseObject>(indexStart + right.Length);
            result.AddRange(left.ToArray().SubArray(0, indexStart));
            result.AddRange(right.ToArray());
            return new Chain(result);
        }

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
                    double delta = Math.Abs(currentAvgRemoteness - averageRemoteness);

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
        private Chain SubChain(Chain source, int start, int end)
        {
            Chain chain = new Chain(end - start + 1);
            for (int i = 0; i < chain.Length; i++)
            {
                chain.Set(source.Get(start + i), i);
            }
            return chain;
        }

        private double FindPercentageOfMatching(Chain first, Chain second)
        {
            int count = 0;
            for (int i = 0; i < second.Length; i++)
            {
                if (first.Get(i).Equals(second.Get(i)))
                {
                    count++;
                }
            }

            return (double)count / second.Length;
        }

        private struct ContenderValue
        {
            public double CurrentAverageRemoteness;
            public Chain PredictedWord;
        }
    }
}