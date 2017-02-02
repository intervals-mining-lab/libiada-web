namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;

    using Newtonsoft.Json;
    using Models.Repositories.Sequences;

    /// <summary>
    /// The subsequences comparer controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesComparerController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesComparerController"/> class.
        /// </summary>
        public SubsequencesComparerController() : base("Subsequences comparer")
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
                ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(2, int.MaxValue, "Compare"));
            }

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="subsequencesCharacteristicTypeLinkId">
        /// The subsequences characteristic type link id.
        /// </param>
        /// <param name="features">
        /// The feature ids.
        /// </param>
        /// <param name="maxPercentageDifference">
        /// The precision.
        /// </param>
        /// <param name="characteristicValueFrom">
        /// Minimum value for calculating characteristic
        /// </param>
        /// <param name="characteristicValueTo">
        /// Maximum value for calculating characteristic
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if count of matters is not 2.
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int characteristicTypeLinkId,
            int subsequencesCharacteristicTypeLinkId,
            Feature[] features,
            string maxPercentageDifference,
            double characteristicValueFrom,
            double characteristicValueTo)
        {
            return Action(() =>
            {
                var attributeValues = new List<AttributeValue>();
                var characteristics = new SubsequenceData[matterIds.Length][];
                string characteristicName;

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];
                IFullCalculator subsequencesCalculator;
                Link subsequencesLink;

                string sequenceCharacteristicName;

                int mattersCount = matterIds.Length;
                int[] subsequencesCount = new int[mattersCount];

                using (var db = new LibiadaWebEntities())
                {
                    // Sequences characteristic
                    var commonSequenceRepository = new CommonSequenceRepository(db);
                    var chains = commonSequenceRepository.GetNucleotideChains(matterIds);

                    var sequencesCharacteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
                    sequenceCharacteristicName = sequencesCharacteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId);

                    var sequencesCalculator = CalculatorsFactory.CreateFullCalculator(sequencesCharacteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName);
                    var sequencesLink = sequencesCharacteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                    // Sequences characterstic
                    double[] completeGenomesCharacteristics = SequencesCharacteristicsCalculator.Calculate(chains, sequencesCalculator, sequencesLink, characteristicTypeLinkId);

                    var matterCharacteristics = new KeyValuePair<long, double>[matterIds.Length];

                    for (int i = 0; i < completeGenomesCharacteristics.Length; i++)
                    {
                        matterCharacteristics[i] = new KeyValuePair<long, double>(matterIds[i], completeGenomesCharacteristics[i]);
                    }

                    matterIds = matterCharacteristics.OrderBy(mc => mc.Value).Select(mc => mc.Key).ToArray();

                    // Subsequences characteristics
                    var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                            .Where(s => s.Notation == Notation.Nucleotides && matterIds.Contains(s.MatterId))
                                            .Select(s => new { s.Id, s.MatterId, MatterName = s.Matter.Name })
                                            .ToDictionary(s => s.Id);
                    parentSequenceIds = parentSequences.OrderBy(ps => Array.IndexOf(matterIds, ps.Value.MatterId)).Select(ps => ps.Key).ToArray();

                    for (int n = 0; n < parentSequenceIds.Length; n++)
                    {
                        matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                    }

                    var subsequencesCharacteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);

                    characteristicName = subsequencesCharacteristicTypeLinkRepository.GetCharacteristicName(subsequencesCharacteristicTypeLinkId);
                    string className = subsequencesCharacteristicTypeLinkRepository.GetCharacteristicType(subsequencesCharacteristicTypeLinkId).ClassName;
                    subsequencesCalculator = CalculatorsFactory.CreateFullCalculator(className);
                    subsequencesLink = subsequencesCharacteristicTypeLinkRepository.GetLibiadaLink(subsequencesCharacteristicTypeLinkId);
                }
                
                // cycle through matters; first level of characteristics array
                for (int i = 0; i < parentSequenceIds.Length; i++)
                {
                    var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            new[] { subsequencesCharacteristicTypeLinkId },
                            features,
                            parentSequenceIds[i],
                            new[] { subsequencesCalculator },
                            new[] { subsequencesLink },
                            attributeValues);

                    subsequencesCount[i] = subsequencesData.Length;

                    subsequencesData = subsequencesData.Where(c => (characteristicValueFrom == 0 && characteristicValueTo == 0)
                        || (c.CharacteristicsValues[0] >= characteristicValueFrom && c.CharacteristicsValues[0] <= characteristicValueTo))
                        .OrderBy(c => c.CharacteristicsValues[0]).ToArray();

                    characteristics[i] = subsequencesData;
                }

                double decimalDifference = double.Parse(maxPercentageDifference, CultureInfo.InvariantCulture) / 100;

                var similarities = new object[mattersCount, mattersCount];

                var equalElements = new List<SubsequenceComparisonData>();

                for (int i = 0; i < characteristics.Length; i++)
                {
                    for (int j = 0; j < characteristics.Length; j++)
                    {
                        double similarSequencesCharacteristicValueFirst = 0;
                        var similarSequencesCharacteristicValueSecond = new Dictionary<int, double>();

                        int secondArrayStartPosition = 0;
                        double differenceSum = 0;

                        int equalElementsCountFromFirst = 0;
                        var equalElementsCountFromSecond = new Dictionary<int, int>();

                        int equalPairsCount = 0;

                        for (int k = 0; k < characteristics[i].Length; k++)
                        {
                            bool equalFound = false;
                            double first = characteristics[i][k].CharacteristicsValues[0];

                            for (int l = secondArrayStartPosition; l < characteristics[j].Length; l++)
                            {
                                double second = characteristics[j][l].CharacteristicsValues[0];

                                double difference = CalculateAverageDifference(first, second);
                                bool nextElementInSecondArrayIsEqual = false;

                                if (l < characteristics[j].Length - 1)
                                {
                                    nextElementInSecondArrayIsEqual = CalculateAverageDifference(second, characteristics[j][l + 1].CharacteristicsValues[0]) <= decimalDifference;
                                }

                                if (difference <= decimalDifference)
                                {
                                    equalFound = true;
                                    equalPairsCount++;

                                    if (!equalElementsCountFromSecond.ContainsKey(l))
                                    {
                                        equalElementsCountFromSecond.Add(l, 1);
                                        differenceSum += difference;
                                    }

                                    if (!similarSequencesCharacteristicValueSecond.ContainsKey(l))
                                    {
                                        similarSequencesCharacteristicValueSecond.Add(l, second);
                                    }

                                    if (i != j)
                                    {
                                        equalElements.Add(new SubsequenceComparisonData
                                        {
                                            Difference = difference,
                                            FirstMatterId = i,
                                            SecondMatterId = j,
                                            FirstSubsequenceId = k,
                                            SecondSubsequenceId = l,
                                        });
                                    }

                                    if (!nextElementInSecondArrayIsEqual)
                                    {
                                        break;
                                    }
                                }
                                else if (second < first)
                                {
                                    secondArrayStartPosition++;
                                }
                            }

                            if (equalFound)
                            {
                                equalElementsCountFromFirst++;
                                similarSequencesCharacteristicValueFirst += first;
                            }
                        }

                        double differenceSecondFinal = equalElementsCountFromSecond.Sum(s => s.Value);
                        double differenceFinal = equalElementsCountFromFirst < differenceSecondFinal ? equalElementsCountFromFirst * 2d : differenceSecondFinal * 2d;

                        double formula1 = differenceFinal / (subsequencesCount[i] + subsequencesCount[j]);

                        double formula2 = 0;
                        if (equalPairsCount != 0 && formula1 != 0)
                        {
                            formula2 = (differenceSum / equalPairsCount) / formula1;
                        }

                        double similarSequencesCharacteristicValueSecondFinal = similarSequencesCharacteristicValueSecond.Sum(s => s.Value);
                        double similarSequencesCharacteristicValue = similarSequencesCharacteristicValueFirst < similarSequencesCharacteristicValueSecondFinal ?
                                            similarSequencesCharacteristicValueFirst * 2d : similarSequencesCharacteristicValueSecondFinal * 2d;

                        double formula3 = similarSequencesCharacteristicValue / (characteristics[i].Sum(c => c.CharacteristicsValues[0]) + characteristics[j].Sum(c => c.CharacteristicsValues[0]));

                        const int digits = 5;

                        similarities[i, j] = new
                        {
                            formula1 = Math.Round(formula1, digits),
                            formula2 = Math.Round(formula2, digits),
                            formula3 = Math.Round(formula3, digits)
                        };
                    }
                }

                var result = new Dictionary<string, object>
                {
                    { "mattersNames", matterNames },
                    { "characteristicName", characteristicName },
                    { "similarities", similarities },
                    { "characteristics", characteristics },
                    { "equalElements", equalElements.OrderBy(e => e.Difference).ToList() },
                    { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                    { "attributeValues", attributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) },
                    { "attributes", ArrayExtensions.ToArray<LibiadaWeb.Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                    { "maxPercentageDifference", maxPercentageDifference },
                    { "sequenceCharacteristicName", sequenceCharacteristicName }
                };

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }

        /// <summary>
        /// Calculates average difference.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private double CalculateAverageDifference(double first, double second)
        {
            return Math.Abs((first - second) / ((first + second) / 2));
        }
    }
}
