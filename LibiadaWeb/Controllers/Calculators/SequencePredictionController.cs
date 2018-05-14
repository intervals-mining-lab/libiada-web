namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;
    using System.Collections;
    using LibiadaWeb.Models.CalculatorsData;

    /// <summary>
    /// The sequence prediction controller.
    /// </summary>
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
                var viewData = viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, 1, "Calculate");
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
            double accuracy)
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
                    mattersName = db.Matter.Single(m => matterId == m.Id).Name;
                    var sequenceId = db.CommonSequence.Single(c => matterId == c.MatterId && c.Notation == notation).Id;
                    sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);

                    var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                    characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);

                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                    calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
                    link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                }

                // characteristics = SequencesCharacteristicsCalculator.Calculate( new[] { sequenceId }, characteristicLinkId);

                CutRule cutRule = new CutRuleWithFixedStart(sequence.GetLength(), step);

                Depth depthCaulc = new Depth();

                CutRuleIterator iter = cutRule.GetIterator();

                var fragments = new List<Chain>();
                var partNames = new List<string>();
                var lengthes = new List<int>();
                var teoreticalDepht = new List<double>();

                while (iter.Next())
                {
                    var fragment = new Chain(iter.GetEndPosition() - iter.GetStartPosition());

                    for (int k = 0; iter.GetStartPosition() + k < iter.GetEndPosition(); k++)
                    {
                        fragment.Set(sequence[iter.GetStartPosition() + k], k);
                    }

                    fragments.Add(fragment);
                    partNames.Add(fragment.ToString());
                    lengthes.Add(fragment.GetLength());

                    teoreticalDepht.Add(depthCaulc.Calculate(fragment, Link.Start));
                }

                characteristics = new double[fragments.Count];
                for (int k = 0; k < fragments.Count; k++)
                {
                    characteristics[k] = calculator.Calculate(fragments[k], link);


                    // fragmentsData[k] = new FragmentData(characteristics, fragments[k].ToString(), starts[i][k], fragments[k].GetLength());
                }




                // var predicted = new List<Chain>();

                //int[] startingPart = new int[initialLength];


                Chain predicted = new Chain(initialLength);


                for (int i = 0; i < initialLength; i++)
                {
                    predicted.Set(sequence[i], i);
                }

                Alphabet alphabet = sequence.Alphabet;
                IEnumerator enumerator = alphabet.GetEnumerator();
                var sequencePredictionResult = new List<SequencePredictionData>();

                for (int i = initialLength; i < sequence.GetLength(); i++)
                {
                    Chain temp = new Chain(initialLength + i);
                    for (int j = 0; j < initialLength + i - 1; j++)
                    {
                        temp.Set(predicted[j], j);
                    }
                    predicted = temp;
                    double depth;
                    do
                    {
                        predicted.Set((IBaseObject)enumerator.Current, i);
                        depth = depthCaulc.Calculate(predicted, Link.Start);
                        if (System.Math.Abs(depth - teoreticalDepht.ElementAt(i)) <= accuracy)
                        {
                            break;
                        }
                    } while (enumerator.MoveNext());

                    sequencePredictionResult.Add(new SequencePredictionData
                    {
                        Fragment = fragments.ElementAt(i).ToString(),
                        Predicted = enumerator.Current.ToString(),
                        ActualCharacteristic = depth,
                        TheoreticalCharacteristic = teoreticalDepht.ElementAt(i)
                    });
                }

                /*int equal = 0;
                for (int i = initialLength; i < sequence.GetLength(); i++)
                {
                    if (sequence[i] == predicted[i])
                    {
                        equal++;
                    }
                }


                double accuracyPercentage = equal / (sequence.GetLength() - initialLength);*/


                // TODO: sequence prediction


                var result = new Dictionary<string, object>
                                 {
                                         { "result", sequencePredictionResult }
                                 };

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }
    }
}