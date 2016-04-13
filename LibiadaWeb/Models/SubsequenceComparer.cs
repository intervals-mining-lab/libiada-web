namespace LibiadaWeb.Models
{
    using System.Collections.Generic;
    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;

    public class SubsequenceComparer
    {
        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        public SubsequenceComparer(LibiadaWebEntities db)
        {
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        public double CompareSequencesBySubsequences(int characteristicTypeLinkId, int notationId, Chain[] firstChains, Chain[] secondChains, double difference, string excludeType)
        {
            var firstSequenceCharacteristics = CalculateCharacteristic(characteristicTypeLinkId, firstChains);

            var secondSequenceCharacteristics = CalculateCharacteristic(characteristicTypeLinkId, secondChains);

            var similarSubsequences = new List<IntPair>();

            for (int i = 0; i < firstSequenceCharacteristics.Count; i++)
            {
                for (int j = 0; j < secondSequenceCharacteristics.Count; j++)
                {
                    if (System.Math.Abs(firstSequenceCharacteristics[i] - secondSequenceCharacteristics[j]) <= difference)
                    {
                        similarSubsequences.Add(new IntPair(i, j));

                        if (excludeType == "Exclude")
                        {
                            firstSequenceCharacteristics[i] = double.NaN;
                            secondSequenceCharacteristics[j] = double.NaN;
                        }
                    }
                }
            }

            return similarSubsequences.Count * 200d / (firstChains.Length + secondChains.Length);
        }

        private List<double> CalculateCharacteristic(int characteristicTypeLinkId, Chain[] sequences)
        {
            var characteristics = new List<double>();
            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

            for (int j = 0; j < sequences.Length; j++)
            {
                {
                    characteristics.Add(calculator.Calculate(sequences[j], link));
                }
            }

            return characteristics;
        }
    }
}
