namespace LibiadaWeb.Models
{
    using System.Collections.Generic;
    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The subsequence comparer.
    /// </summary>
    public class SubsequenceComparer
    {
        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceComparer"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public SubsequenceComparer(LibiadaWebEntities db)
        {
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        /// <summary>
        /// The compare sequences by subsequences.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="firstChains">
        /// The first chains.
        /// </param>
        /// <param name="secondChains">
        /// The second chains.
        /// </param>
        /// <param name="difference">
        /// The difference.
        /// </param>
        /// <param name="excludeType">
        /// The exclude type.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
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

        /// <summary>
        /// The calculate characteristic.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="sequences">
        /// The sequences.
        /// </param>
        /// <returns>
        /// The <see cref="List{Double}"/>.
        /// </returns>
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
