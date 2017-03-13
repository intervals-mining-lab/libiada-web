namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Models.Repositories.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The sequences characteristics calculator.
    /// </summary>
    public static class SequencesCharacteristicsCalculator
    {
        /// <summary>
        /// Calculation method.
        /// </summary>
        /// <param name="chains">
        /// The chains.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[][]"/>.
        /// </returns>
        public static double[][] Calculate(Chain[][] chains, short[] characteristicTypeLinkIds)
        {
            var newCharacteristics = new List<CharacteristicValue>();
            var characteristics = new double[chains.Length][];
            var sequenceIds = chains.SelectMany(c => c).Select(c => c.Id).Distinct();
            var links = new Link[characteristicTypeLinkIds.Length];
            var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];

            Dictionary<long, Dictionary<short, double>> dbCharacteristics;
            using (var db = new LibiadaWebEntities())
            {
                dbCharacteristics = db.CharacteristicValue
                                              .Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && sequenceIds.Contains(c.SequenceId))
                                              .ToArray()
                                              .GroupBy(c => c.SequenceId)
                                              .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

                var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    links[k] = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[k]);
                    string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[k]).ClassName;
                    calculators[k] = FullCalculatorsFactory.CreateFullCalculator(className);
                }
            }

            for (int i = 0; i < chains.Length; i++)
            {
                characteristics[i] = new double[calculators.Length];

                for (int j = 0; j < calculators.Length; j++)
                {
                    long sequenceId = chains[i][j].Id;
                    chains[i][j].FillIntervalManagers();

                    Dictionary<short, double> sequenceDbCharacteristics;
                    if (!dbCharacteristics.TryGetValue(sequenceId, out sequenceDbCharacteristics))
                    {
                        sequenceDbCharacteristics = new Dictionary<short, double>();
                    }

                    if (!sequenceDbCharacteristics.TryGetValue(characteristicTypeLinkIds[j], out characteristics[i][j]))
                    {
                        characteristics[i][j] = calculators[j].Calculate(chains[i][j], links[j]);
                        var currentCharacteristic = new CharacteristicValue
                        {
                            SequenceId = sequenceId,
                            CharacteristicTypeLinkId = characteristicTypeLinkIds[j],
                            Value = characteristics[i][j]
                        };

                        newCharacteristics.Add(currentCharacteristic);
                    }
                }
            }

            // trying to save calculated characteristics to database
            using (var db = new LibiadaWebEntities())
            {
                var characteristicRepository = new CharacteristicRepository(db);
                characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
            }

            return characteristics;
        }

        /// <summary>
        /// Calculation method.
        /// </summary>
        /// <param name="chains">
        /// The chains.
        /// </param>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[]"/>.
        /// </returns>
        public static double[] Calculate(Chain[] chains, short characteristicTypeLinkId)
        {
            var twoDimensionalChains = new Chain[chains.Length][];
            for (int i = 0; i < chains.Length; i++)
            {
                twoDimensionalChains[i] = new[] { chains[i] };
            }

            double[][] twoDimensionalResult = Calculate(twoDimensionalChains, new[] { characteristicTypeLinkId });

            var result = new double[chains.Length];
            for (int i = 0; i < twoDimensionalResult.Length; i++)
            {
                result[i] = twoDimensionalResult[i][0];
            }

            return result;
        }

        /// <summary>
        /// Calculation method.
        /// </summary>
        /// <param name="chains">
        /// The chains.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="rotate">
        /// The rotate flag.
        /// </param>
        /// <param name="complementary">
        /// The complementary flag.
        /// </param>
        /// <param name="rotationLength">
        /// The rotation length.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[][]"/>.
        /// </returns>
        public static double[][] Calculate(Chain[][] chains, short[] characteristicTypeLinkIds, bool rotate, bool complementary, uint? rotationLength)
        {
            var links = new Link[characteristicTypeLinkIds.Length];
            var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];
            var characteristics = new double[chains.Length][];

            using (var db = new LibiadaWebEntities())
            {
                var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    links[k] = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[k]);
                    string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[k]).ClassName;
                    calculators[k] = FullCalculatorsFactory.CreateFullCalculator(className);
                }
            }

            for (int i = 0; i < chains.Length; i++)
            {
                characteristics[i] = new double[calculators.Length];

                for (int j = 0; j < calculators.Length; j++)
                {
                    if (complementary)
                    {
                        var sourceSequence = new Sequence(Alphabets.DNA, chains[i][j].ToString());
                        var complementarySequence = sourceSequence.GetReverseComplementedSequence();
                        chains[i][j] = new Chain(complementarySequence.ConvertToString());
                    }

                    if (rotate)
                    {
                        var building = chains[i][j].Building.Rotate(rotationLength ?? 0);
                        var newSequence = building.Select(t => new ValueInt(t)).Cast<IBaseObject>().ToList();
                        chains[i][j] = new Chain(newSequence);
                    }

                    chains[i][j].FillIntervalManagers();

                    characteristics[i][j] = calculators[j].Calculate(chains[i][j], links[j]);
                }
            }

            return characteristics;
        }
    }
}
