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
        /// <param name="chainIds">
        /// The chains ids.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristicLink ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[][]"/>.
        /// </returns>
        public static double[][] Calculate(long[][] chainIds, short[] characteristicLinkIds)
        {
            Dictionary<long, short[]> chainCharacteristicsIds = ToSequenceCharacteristicsIdsDictionary(chainIds, characteristicLinkIds);
            Dictionary<long, Dictionary<short, double>> result = Calculate(chainCharacteristicsIds);
            return ExtractCharacteristicsValues(result, chainIds, characteristicLinkIds);
        }

        /// <summary>
        /// Calculation method.
        /// </summary>
        /// <param name="chainCharacteristicsIds">
        /// Dictionary with chain ids as a key
        /// and characteristicLink ids array as value.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[][]"/>.
        /// </returns>
        public static Dictionary<long, Dictionary<short, double>> Calculate(Dictionary<long, short[]> chainCharacteristicsIds)
        {
            var newCharacteristics = new List<CharacteristicValue>();
            var allCharacteristics = new Dictionary<long, Dictionary<short, double>>();

            using (var db = new LibiadaWebEntities())
            {
                short[] characteristicLinkIds = chainCharacteristicsIds.SelectMany(c => c.Value).Distinct().ToArray();
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                var calculators = new Dictionary<short, LinkedFullCalculator>();
                foreach (short characteristicLinkId in characteristicLinkIds)
                {
                    Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                    calculators.Add(characteristicLinkId, new LinkedFullCalculator(characteristic, link));
                }

                var commonSequenceRepository = new CommonSequenceRepository(db);
                long[] sequenceIds = chainCharacteristicsIds.Keys.ToArray();
                foreach (long sequenceId in sequenceIds)
                {
                    short[] sequenceCharacteristicLinkIds = chainCharacteristicsIds[sequenceId];
                    Dictionary<short, double> characteristics = db.CharacteristicValue
                                                                  .Where(c => sequenceId == c.SequenceId && sequenceCharacteristicLinkIds.Contains(c.CharacteristicLinkId))
                                                                  .ToDictionary(ct => ct.CharacteristicLinkId, ct => ct.Value);

                    allCharacteristics.Add(sequenceId, characteristics);

                    if (characteristics.Count < sequenceCharacteristicLinkIds.Length)
                    {
                        Chain sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);
                        sequence.FillIntervalManagers();

                        foreach (short sequenceCharacteristicLinkId in sequenceCharacteristicLinkIds)
                        {
                            if (!characteristics.ContainsKey(sequenceCharacteristicLinkId))
                            {
                                LinkedFullCalculator calculator = calculators[sequenceCharacteristicLinkId];
                                double characteristicValue = calculator.Calculate(sequence);
                                var characteristic = new CharacteristicValue
                                {
                                    SequenceId = sequenceId,
                                    CharacteristicLinkId = sequenceCharacteristicLinkId,
                                    Value = characteristicValue
                                };

                                characteristics.Add(sequenceCharacteristicLinkId, characteristicValue);
                                newCharacteristics.Add(characteristic);
                            }
                        }
                    }
                }

                var characteristicRepository = new CharacteristicRepository(db);
                characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

                return allCharacteristics;
            }
        }

        /// <summary>
        /// Calculation method.
        /// </summary>
        /// <param name="chainIds">
        /// The chains ids.
        /// </param>
        /// <param name="characteristicLinkId">
        /// The characteristicLink id.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[]"/>.
        /// </returns>
        public static double[] Calculate(long[] chainIds, short characteristicLinkId)
        {
            Dictionary<long, short[]> chainCharacteristicsIds = chainIds.ToDictionary(c => c, c => new[] { characteristicLinkId });

            Dictionary<long, Dictionary<short, double>> dictionaryResult = Calculate(chainCharacteristicsIds);

            var result = new double[chainIds.Length];
            for (int i = 0; i < dictionaryResult.Count; i++)
            {
                result[i] = dictionaryResult[chainIds[i]][characteristicLinkId];
            }

            return result;
        }

        /// <summary>
        /// Calculation method.
        /// </summary>
        /// <param name="chainIds">
        /// The chains ids.
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
            long[] sequenceIds = chainIds.SelectMany(c => c).Distinct().ToArray();
            var sequences = new Dictionary<long, Chain>();

            using (var db = new LibiadaWebEntities())
            {
                var commonSequenceRepository = new CommonSequenceRepository(db);
                for (int i = 0; i < sequenceIds.Length; i++)
                {
                    sequences.Add(sequenceIds[i], commonSequenceRepository.GetLibiadaChain(sequenceIds[i]));
                }

                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    links[k] = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkIds[k]);
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkIds[k]);
                    calculators[k] = FullCalculatorsFactory.CreateCalculator(characteristic);
                }
            }

            for (int i = 0; i < chainIds.Length; i++)
            {
                characteristics[i] = new double[calculators.Length];

                for (int j = 0; j < calculators.Length; j++)
                {
                    long sequenceId = chainIds[i][j];
                    var sequence = (Chain)sequences[sequenceId].Clone();
                    if (complementary)
                    {
                        var sourceSequence = new Sequence(Alphabets.DNA, sequence.ToString());
                        ISequence complementarySequence = sourceSequence.GetReverseComplementedSequence();
                        sequence = new Chain(complementarySequence.ConvertToString());
                    }

                    if (rotate)
                    {
                        int[] building = sequence.Building.Rotate(rotationLength ?? 0);
                        List<IBaseObject> newSequence = building.Select(t => new ValueInt(t)).ToList<IBaseObject>();
                        sequence = new Chain(newSequence);
                    }

                    sequence.FillIntervalManagers();

                    characteristics[i][j] = calculators[j].Calculate(sequence, links[j]);
                }
            }

            return characteristics;
        }

        /// <summary>
        /// Converts data into dictionary with sequences ids as keys
        /// and characteristicLinks ids as values.
        /// </summary>
        /// <param name="sequenceIds">
        /// The sequence ids.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristic link ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:Dictionary{long,short[]}"/>.
        /// </returns>
        private static Dictionary<long, short[]> ToSequenceCharacteristicsIdsDictionary(long[][] sequenceIds, short[] characteristicLinkIds)
        {
            Dictionary<long, List<short>> result = sequenceIds.SelectMany(s => s)
                                                              .Distinct()
                                                              .ToDictionary(s => s, s => new List<short>());
            for (int i = 0; i < sequenceIds.Length; i++)
            {
                for (int j = 0; j < sequenceIds[i].Length; j++)
                {
                    result[sequenceIds[i][j]].Add(characteristicLinkIds[j]);
                }
            }

            return result.ToDictionary(c => c.Key, c => c.Value.ToArray());
        }

        /// <summary>
        /// Extracts two-dimensional array of characteristics values.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        /// <param name="sequenceIds">
        /// The sequence ids.
        /// </param>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <returns>
        /// The <see cref="T:double[][]"/>.
        /// </returns>
        private static double[][] ExtractCharacteristicsValues(Dictionary<long, Dictionary<short, double>> characteristics, long[][] sequenceIds, short[] characteristicIds)
        {
            var result = new double[sequenceIds.Length][];
            for (int i = 0; i < sequenceIds.Length; i++)
            {
                result[i] = new double[sequenceIds[i].Length];
                for (int j = 0; j < characteristicIds.Length; j++)
                {
                    result[i][j] = characteristics[sequenceIds[i][j]][characteristicIds[j]];
                }
            }

            return result;
        }
    }
}
