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
        public static double[] CalculateSubsequencesCharacteristics(long parentId, short characteristicId, Feature[] features, string[] filters = null)
        {
            var characteristicsIds = new[] { characteristicId };
            var attributes = new List<AttributeValue>();
            var subsequencesData = CalculateSubsequencesCharacteristics(characteristicsIds, features, parentId, attributes, filters);
            return subsequencesData.Select(s => s.CharacteristicsValues[0]).ToArray();
        }

        /// <summary>
        /// Calculates subsequences characteristics.
        /// </summary>
        /// <param name="characteristicIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="features">
        /// The features ids.
        /// </param>
        /// <param name="parentSequenceId">
        /// The parent sequence id.
        /// </param>
        /// <param name="attributeValues">
        /// non-redundant array of all attributes.
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
            List<AttributeValue> attributeValues,
            string[] filters = null)
        {
            Chain[] sequences;
            long[] subsequenceIds;
            SubsequenceData[] subsequenceData;
            Dictionary<long, AttributeValue[]> subsequencesAttributes;
            Dictionary<long, Dictionary<short, double>> characteristics;
            var calculators = new IFullCalculator[characteristicIds.Length];
            var links = new Link[characteristicIds.Length];
            var newCharacteristics = new List<CharacteristicValue>();

            // creating local context to avoid memory overflow due to possibly big cache of characteristics
            using (var db = new LibiadaWebEntities())
            {
                var subsequenceExtractor = new SubsequenceExtractor(db);
                var sequenceAttributeRepository = new SequenceAttributeRepository(db);

                Subsequence[] subsequences = filters == null ?
                    subsequenceExtractor.GetSubsequences(parentSequenceId, features) :
                    subsequenceExtractor.GetSubsequences(parentSequenceId, features, filters);

                subsequenceData = subsequences.Select(s => new SubsequenceData(s)).ToArray();

                // converting to libiada sequences
                sequences = subsequenceExtractor.ExtractChains(parentSequenceId, subsequences);

                subsequenceIds = subsequences.Select(s => s.Id).ToArray();
                subsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);

                characteristics = db.CharacteristicValue
                        .Where(c => characteristicIds.Contains(c.CharacteristicLinkId) && subsequenceIds.Contains(c.SequenceId))
                        .ToArray()
                        .GroupBy(c => c.SequenceId)
                        .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicLinkId, ct => ct.Value));

                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                for (int k = 0; k < characteristicIds.Length; k++)
                {
                    short characteristicLinkId = characteristicIds[k];
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                    calculators[k] = FullCalculatorsFactory.CreateCalculator(characteristic);
                    links[k] = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                }
            }

            // cycle through subsequences
            for (int i = 0; i < sequences.Length; i++)
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
                        values[j] = calculators[j].Calculate(sequences[i], links[j]);
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

                subsequenceData[i].Attributes = FillAttributesIndexes(attributeValues, subsequencesAttributes, subsequenceIds[i]);
            }

            using (var db = new LibiadaWebEntities())
            {
                // trying to save calculated characteristics to database
                var characteristicRepository = new CharacteristicRepository(db);
                characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
            }

            return subsequenceData;
        }

        /// <summary>
        /// Gets attributes indexes.
        /// </summary>
        /// <param name="attributeValues">
        /// The attribute values.
        /// </param>
        /// <param name="subsequencesAttributes">
        /// The db subsequences attributes.
        /// </param>
        /// <param name="subsequenceId">
        /// The subsequence id.
        /// </param>
        /// <returns>
        /// The <see cref="T:int[]"/>.
        /// </returns>
        private static int[] FillAttributesIndexes(List<AttributeValue> attributeValues, Dictionary<long, AttributeValue[]> subsequencesAttributes, long subsequenceId)
        {
            subsequencesAttributes.TryGetValue(subsequenceId, out AttributeValue[] attributes);
            attributes = attributes ?? new AttributeValue[0];

            var attributesIndexes = new int[attributes.Length];
            for (int j = 0; j < attributes.Length; j++)
            {
                if (!attributeValues.Contains(attributes[j]))
                {
                    attributeValues.Add(attributes[j]);
                }

                attributesIndexes[j] = attributeValues.IndexOf(attributes[j]);
            }

            return attributesIndexes;
        }
    }
}
