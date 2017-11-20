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
        /// <param name="characteristicLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="features">
        /// The features ids.
        /// </param>
        /// <param name="parentSequenceId">
        /// The parent sequence id.
        /// </param>
        /// <param name="attributeValues">
        /// Nonredundant array of all attributes.
        /// </param>
        /// <param name="filters">
        /// Textual search filters for subsequences products.
        /// </param>
        /// <returns>
        /// The <see cref="T:SubsequenceData[]"/>.
        /// </returns>
        public static SubsequenceData[] CalculateSubsequencesCharacteristics(
            short[] characteristicLinkIds,
            Feature[] features,
            long parentSequenceId,
            List<AttributeValue> attributeValues,
            string[] filters = null)
        {
            Chain[] sequences;
            long[] subsequenceIds;
            SubsequenceData[] subsequenceData;
            Dictionary<long, AttributeValue[]> dbSubsequencesAttributes;
            Dictionary<long, Dictionary<short, double>> dbCharacteristics;
            var calculators = new IFullCalculator[characteristicLinkIds.Length];
            var links = new Link[characteristicLinkIds.Length];
            var newCharacteristics = new List<CharacteristicValue>();

            // creating local context to avoid memory overflow due to possibly big cache of characteristics
            using (var db = new LibiadaWebEntities())
            {
                var subsequenceExtractor = new SubsequenceExtractor(db);
                var sequenceAttributeRepository = new SequenceAttributeRepository(db);

                Subsequence[] dbSubsequences = filters == null ?
                    subsequenceExtractor.GetSubsequences(parentSequenceId, features) :
                    subsequenceExtractor.GetSubsequences(parentSequenceId, features, filters);

                subsequenceData = dbSubsequences.Select(s => new SubsequenceData(s)).ToArray();

                // converting to libiada sequences
                sequences = subsequenceExtractor.ExtractChains(parentSequenceId, dbSubsequences);

                subsequenceIds = dbSubsequences.Select(s => s.Id).ToArray();
                dbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);

                dbCharacteristics = db.CharacteristicValue
                        .Where(c => characteristicLinkIds.Contains(c.CharacteristicLinkId) && subsequenceIds.Contains(c.SequenceId))
                        .ToArray()
                        .GroupBy(c => c.SequenceId)
                        .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicLinkId, ct => ct.Value));

                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    short characteristicLinkId = characteristicLinkIds[k];
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                    calculators[k] = FullCalculatorsFactory.CreateCalculator(characteristic);
                    links[k] = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                }
            }

            // cycle through subsequences
            for (int i = 0; i < sequences.Length; i++)
            {
                var values = new double[calculators.Length];
                Dictionary<short, double> sequenceDbCharacteristics;
                if (!dbCharacteristics.TryGetValue(subsequenceIds[i], out sequenceDbCharacteristics))
                {
                    sequenceDbCharacteristics = new Dictionary<short, double>();
                }

                // cycle through characteristics and notations
                for (int j = 0; j < calculators.Length; j++)
                {
                    short characteristicLinkId = characteristicLinkIds[j];
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

                subsequenceData[i].Attributes = FillAttributesIndexes(attributeValues, dbSubsequencesAttributes, subsequenceIds[i]);
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
        /// <param name="dbSubsequencesAttributes">
        /// The db subsequences attributes.
        /// </param>
        /// <param name="subsequenceId">
        /// The subsequence id.
        /// </param>
        /// <returns>
        /// The <see cref="T:int[]"/>.
        /// </returns>
        private static int[] FillAttributesIndexes(List<AttributeValue> attributeValues, Dictionary<long, AttributeValue[]> dbSubsequencesAttributes, long subsequenceId)
        {
            AttributeValue[] attributes;
            if (!dbSubsequencesAttributes.TryGetValue(subsequenceId, out attributes))
            {
                attributes = new AttributeValue[0];
            }

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
