namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;

    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The subsequences characteristics calculator.
    /// </summary>
    public static class SubsequencesCharacteristicsCalculator
    {
        /// <summary>
        /// Calculates subsequences characteristics.
        /// </summary>
        /// <param name="parentId">
        /// The parent sequence id.
        /// </param>
        /// <param name="characteristicId">
        /// The id of characteristic type, arrangement type and link as <see cref="FullCharacteristicLink"/>.
        /// </param>
        /// <param name="features">
        /// The  features ids of subsequences to extract.
        /// </param>
        /// <param name="filters">
        /// Textual search filters for subsequences products.
        /// Null by default.
        /// </param>
        /// <returns></returns>
        public static double[] CalculateSubsequencesCharacteristics(long parentId, short characteristicId, Feature[] features, string[] filters = null)
        {
            var characteristicsIds = new[] { characteristicId };
            SubsequenceData[] subsequencesData = CalculateSubsequencesCharacteristics(characteristicsIds, features, parentId, filters);
            return subsequencesData.Select(s => s.CharacteristicsValues[0]).ToArray();
        }

        /// <summary>
        /// Calculates subsequences characteristics.
        /// </summary>
        /// <param name="characteristicIds">
        /// The ids of characteristic types, arrangement types and links as <see cref="FullCharacteristicLink"/>.
        /// </param>
        /// <param name="features">
        /// The  features ids of subsequences to extract.
        /// </param>
        /// <param name="parentSequenceId">
        /// The parent sequence id.
        /// </param>
        /// <param name="filters">
        /// Textual search filters for subsequences products.
        /// </param>
        /// <returns>
        /// The <see cref="T:SubsequenceData[]"/> .
        /// </returns>
        public static SubsequenceData[] CalculateSubsequencesCharacteristics(
            short[] characteristicIds,
            Feature[] features,
            long parentSequenceId,
            string[] filters = null)
        {
            Dictionary<long, Chain> sequences;
            long[] subsequenceIds;
            SubsequenceData[] subsequenceData;
            Dictionary<long, Dictionary<short, double>> characteristics;
            var calculators = new IFullCalculator[characteristicIds.Length];
            var links = new Link[characteristicIds.Length];
            var newCharacteristics = new List<CharacteristicValue>();

            // creating local context to avoid memory overflow due to possibly big cache of characteristics
            using (var db = new LibiadaWebEntities())
            {
                var subsequenceExtractor = new SubsequenceExtractor(db);

                Subsequence[] subsequences = filters == null ?
                    subsequenceExtractor.GetSubsequences(parentSequenceId, features) :
                    subsequenceExtractor.GetSubsequences(parentSequenceId, features, filters);

                subsequenceData = subsequences.Select(s => new SubsequenceData(s)).ToArray();

                // converting to libiada sequences
                subsequenceIds = subsequences.Select(s => s.Id).ToArray();



                characteristics = db.CharacteristicValue
                        .Where(c => characteristicIds.Contains(c.CharacteristicLinkId) && subsequenceIds.Contains(c.SequenceId))
                        .ToArray()
                        .GroupBy(c => c.SequenceId)
                        .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicLinkId, ct => ct.Value));

                sequences = subsequenceExtractor.GetSubsequences(subsequences);
            }

            var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
            for (int k = 0; k < characteristicIds.Length; k++)
            {
                short characteristicLinkId = characteristicIds[k];
                FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                calculators[k] = FullCalculatorsFactory.CreateCalculator(characteristic);
                links[k] = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
            }

            // cycle through subsequences
            for (int i = 0; i < subsequenceIds.Length; i++)
            {
                characteristics.TryGetValue(subsequenceIds[i], out Dictionary<short, double> sequenceDbCharacteristics);
                sequenceDbCharacteristics = sequenceDbCharacteristics ?? new Dictionary<short, double>();
                var values = new double[calculators.Length];

                // cycle through characteristics and notations
                for (int j = 0; j < calculators.Length; j++)
                {
                    short characteristicLinkId = characteristicIds[j];
                    if (!sequenceDbCharacteristics.TryGetValue(characteristicLinkId, out values[j]))
                    {
                        values[j] = calculators[j].Calculate(sequences[subsequenceIds[i]], links[j]);
                        var currentCharacteristic = new CharacteristicValue
                        {
                            SequenceId = subsequenceIds[i],
                            CharacteristicLinkId = characteristicLinkId,
                            Value = values[j]
                        };

                        newCharacteristics.Add(currentCharacteristic);
                    }
                }

                subsequenceData[i].CharacteristicsValues = values;
            }

            using (var db = new LibiadaWebEntities())
            {
                // trying to save calculated characteristics to database
                var characteristicRepository = new CharacteristicRepository(db);
                characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
            }

            return subsequenceData;
        }

    }
}
