namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaCore.Misc;

    using LibiadaWeb.Models.Repositories.Calculators;

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
        /// <param name="calculators">
        /// The calculators.
        /// </param>
        /// <param name="links">
        /// The links.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[][]"/>.
        /// </returns>
        public static double[][] Calculate(Chain[][] chains, IFullCalculator[] calculators, Link[] links, int[] characteristicTypeLinkIds)
        {
            var newCharacteristics = new List<Characteristic>();
            var characteristics = new double[chains.Length][];
            var sequenceIds = chains.SelectMany(c => c).Select(c => c.Id).Distinct();

            Dictionary<long, Dictionary<int, double>> dbCharacteristics;
            using (var db = new LibiadaWebEntities())
            {
                dbCharacteristics = db.Characteristic
                                              .Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && sequenceIds.Contains(c.SequenceId))
                                              .ToArray()
                                              .GroupBy(c => c.SequenceId)
                                              .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));
            }

            for (int i = 0; i < chains.Length; i++)
            {
                characteristics[i] = new double[calculators.Length];

                for (int j = 0; j < calculators.Length; j++)
                {
                    long sequenceId = chains[i][j].Id;
                    chains[i][j].FillIntervalManagers();

                    Dictionary<int, double> sequenceDbCharacteristics;
                    if (!dbCharacteristics.TryGetValue(sequenceId, out sequenceDbCharacteristics))
                    {
                        sequenceDbCharacteristics = new Dictionary<int, double>();
                    }

                    if (!sequenceDbCharacteristics.TryGetValue(characteristicTypeLinkIds[j], out characteristics[i][j]))
                    {
                        characteristics[i][j] = calculators[j].Calculate(chains[i][j], links[j]);
                        var currentCharacteristic = new Characteristic
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
        /// <param name="calculator">
        /// The calculator.
        /// </param>
        /// <param name="link">
        /// The link.
        /// </param>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[]"/>.
        /// </returns>
        public static double[] Calculate(Chain[] chains, IFullCalculator calculator, Link link, int characteristicTypeLinkId)
        {
            var newCharacteristics = new List<Characteristic>();
            var characteristics = new double[chains.Length];
            var sequenceIds = chains.Select(c => c.Id);

            Dictionary<long, double> dbCharacteristics;
            using (var db = new LibiadaWebEntities())
            {
                dbCharacteristics = db.Characteristic
                                              .Where(c => characteristicTypeLinkId == c.CharacteristicTypeLinkId && sequenceIds.Contains(c.SequenceId))
                                              .ToArray()
                                              .GroupBy(c => c.SequenceId)
                                              .ToDictionary(c => c.Key, c => c.Single().Value);
            }

            for (int i = 0; i < chains.Length; i++)
            {
                chains[i].FillIntervalManagers();

                long sequenceId = chains[i].Id;

                if (!dbCharacteristics.TryGetValue(sequenceId, out characteristics[i]))
                {
                    characteristics[i] = calculator.Calculate(chains[i], link);
                    var currentCharacteristic = new Characteristic
                    {
                        SequenceId = sequenceId,
                        CharacteristicTypeLinkId = characteristicTypeLinkId,
                        Value = characteristics[i]
                    };

                    newCharacteristics.Add(currentCharacteristic);
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
        /// <param name="calculators">
        /// The calculators.
        /// </param>
        /// <param name="links">
        /// The links.
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
        public static double[][] Calculate(Chain[][] chains, IFullCalculator[] calculators, Link[] links, bool rotate, bool complementary, uint? rotationLength)
        {
            var characteristics = new double[chains.Length][];

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
