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
    using LibiadaWeb.Models.Repositories.Sequences;

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
        /// <param name="characteristicLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[][]"/>.
        /// </returns>
        public static double[][] Calculate(long[][] chainIds, short[] characteristicLinkIds)
        {
            IEnumerable<long> sequenceIds = chainIds.SelectMany(c => c).Distinct();
            var newCharacteristics = new List<CharacteristicValue>();
            var characteristics = new double[chainIds.Length][];
            var links = new Link[characteristicLinkIds.Length];
            var calculators = new IFullCalculator[characteristicLinkIds.Length];

            var notCalculatedSequences = new Dictionary<long, Chain>();
            Dictionary<long, Dictionary<short, double>> dbCharacteristics;
            using (var db = new LibiadaWebEntities())
            {
                dbCharacteristics = db.CharacteristicValue
                                      .Where(c => characteristicLinkIds.Contains(c.CharacteristicLinkId) && sequenceIds.Contains(c.SequenceId))
                                      .ToArray()
                                      .GroupBy(c => c.SequenceId)
                                      .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicLinkId, ct => ct.Value));

                var commonSequenceRepository = new CommonSequenceRepository(db);
                foreach(var sequenceCharacteristics in dbCharacteristics)
                {
                    long seqienceId = sequenceCharacteristics.Key;
                    if(sequenceCharacteristics.Value.Count < characteristicLinkIds.Length)
                    {
                        notCalculatedSequences.Add(seqienceId,commonSequenceRepository.GetLibiadaChain(seqienceId));
                        notCalculatedSequences[seqienceId].FillIntervalManagers();
                    }
                }


                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    links[k] = characteristicTypeLinkRepository.GetLinkForFullCharacteristic(characteristicLinkIds[k]);
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetFullCharacteristic(characteristicLinkIds[k]);
                    calculators[k] = FullCalculatorsFactory.CreateCalculator(characteristic);
                }
            }

            for (int i = 0; i < chainIds.Length; i++)
            {
                characteristics[i] = new double[calculators.Length];

                for (int j = 0; j < calculators.Length; j++)
                {
                    long sequenceId = chainIds[i][j];

                    Dictionary<short, double> sequenceDbCharacteristics;
                    if (!dbCharacteristics.TryGetValue(sequenceId, out sequenceDbCharacteristics))
                    {
                        sequenceDbCharacteristics = new Dictionary<short, double>();
                    }

                    if (!sequenceDbCharacteristics.TryGetValue(characteristicLinkIds[j], out characteristics[i][j]))
                    {
                        characteristics[i][j] = calculators[j].Calculate(notCalculatedSequences[sequenceId], links[j]);
                        var currentCharacteristic = new CharacteristicValue
                        {
                            SequenceId = sequenceId,
                            CharacteristicLinkId = characteristicLinkIds[j],
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
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[]"/>.
        /// </returns>
        public static double[] Calculate(long[] chainIds, short characteristicLinkId)
        {
            var twoDimensionalChainIds = new long[chainIds.Length][];
            for (int i = 0; i < chainIds.Length; i++)
            {
                twoDimensionalChainIds[i] = new[] { chainIds[i] };
            }

            double[][] twoDimensionalResult = Calculate(twoDimensionalChainIds, new[] { characteristicLinkId });

            var result = new double[chainIds.Length];
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
        /// <param name="characteristicLinkIds">
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
        public static double[][] Calculate(long[][] chainIds, short[] characteristicLinkIds, bool rotate, bool complementary, uint? rotationLength)
        {
            var links = new Link[characteristicLinkIds.Length];
            var calculators = new IFullCalculator[characteristicLinkIds.Length];
            var characteristics = new double[chainIds.Length][];
            var chains = new Chain[chainIds.Length][];

            using (var db = new LibiadaWebEntities())
            {
                var commonSequenceRepository = new CommonSequenceRepository(db);
                for (int i = 0; i < chainIds.Length; i++)
                {
                    chains[i] = new Chain[chainIds[i].Length];
                    for(int j = 0; j < chainIds[i].Length; j++)
                    {
                        chains[i][j] = commonSequenceRepository.GetLibiadaChain(chainIds[i][j]);
                    }
                }


                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    links[k] = characteristicTypeLinkRepository.GetLinkForFullCharacteristic(characteristicLinkIds[k]);
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetFullCharacteristic(characteristicLinkIds[k]);
                    calculators[k] = FullCalculatorsFactory.CreateCalculator(characteristic);
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
                        ISequence complementarySequence = sourceSequence.GetReverseComplementedSequence();
                        chains[i][j] = new Chain(complementarySequence.ConvertToString());
                    }

                    if (rotate)
                    {
                        int[] building = chains[i][j].Building.Rotate(rotationLength ?? 0);
                        List<IBaseObject> newSequence = building.Select(t => new ValueInt(t)).ToList<IBaseObject>();
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
