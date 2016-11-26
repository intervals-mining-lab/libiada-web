namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Models.Repositories.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.CalculatorsData;

    /// <summary>
    /// The subsequences characteristics calculator.
    /// </summary>
    public static class SubsequencesCharacteristicsCalculator
    {
        /// <summary>
        /// Calculates subsequences characteristics.
        /// </summary>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="featureIds">
        /// The features ids.
        /// </param>
        /// <param name="parentSequenceId">
        /// The parent sequence id.
        /// </param>
        /// <param name="calculators">
        /// The calculators.
        /// </param>
        /// <param name="links">
        /// The links.
        /// </param>
        /// <returns>
        /// The <see cref="T:SubsequenceData[]"/>.
        /// </returns>
        public static SubsequenceData[] CalculateSubsequencesCharacteristics(
            int[] characteristicTypeLinkIds,
            int[] featureIds,
            long parentSequenceId,
            IFullCalculator[] calculators,
            Link[] links,
            List<AttributeValue> attributeValues,
            string[] filters = null)
        {
            // creating local context to avoid memory overflow due to possibly big cache of characteristics
            using (var context = new LibiadaWebEntities())
            {
                var subsequenceExtractor = new SubsequenceExtractor(context);
                var sequenceAttributeRepository = new SequenceAttributeRepository(context);
                var attributeRepository = new AttributeRepository();
                var newCharacteristics = new List<Characteristic>();

                // extracting data from database
                var dbSubsequences = filters == null ? subsequenceExtractor.GetSubsequences(parentSequenceId, featureIds) : subsequenceExtractor.GetSubsequences(parentSequenceId, featureIds, filters);
                var subsequenceIds = dbSubsequences.Select(s => s.Id).ToArray();
                var dbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);

                var dbCharacteristics = context.Characteristic.Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && subsequenceIds.Contains(c.SequenceId))
                        .ToArray()
                        .GroupBy(c => c.SequenceId)
                        .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

                // converting to libiada sequences
                var sequences = subsequenceExtractor.ExtractChains(dbSubsequences, parentSequenceId);
                var subsequenceData = new SubsequenceData[sequences.Length];

                // cycle through subsequences
                for (int i = 0; i < sequences.Length; i++)
                {
                    var values = new double[characteristicTypeLinkIds.Length];
                    Dictionary<int, double> sequenceDbCharacteristics;
                    if (!dbCharacteristics.TryGetValue(dbSubsequences[i].Id, out sequenceDbCharacteristics))
                    {
                        sequenceDbCharacteristics = new Dictionary<int, double>();
                    }

                    // cycle through characteristics and notations
                    for (int j = 0; j < characteristicTypeLinkIds.Length; j++)
                    {
                        int characteristicTypeLinkId = characteristicTypeLinkIds[j];
                        if (!sequenceDbCharacteristics.TryGetValue(characteristicTypeLinkId, out values[j]))
                        {
                            values[j] = calculators[j].Calculate(sequences[i], links[j]);
                            var currentCharacteristic = new Characteristic
                            {
                                SequenceId = dbSubsequences[i].Id,
                                CharacteristicTypeLinkId = characteristicTypeLinkId,
                                Value = values[j]
                            };

                            newCharacteristics.Add(currentCharacteristic);
                        }
                    }

                    AttributeValue[] attributes;
                    if (!dbSubsequencesAttributes.TryGetValue(dbSubsequences[i].Id, out attributes))
                    {
                        attributes = new AttributeValue[0];
                    }

                    var attributeIndexes = new int[attributes.Length];
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if (!attributeValues.Contains(attributes[j]))
                        {
                            attributeValues.Add(attributes[j]);
                        }

                        attributeIndexes[j] = attributeValues.IndexOf(attributes[j]);
                    }

                    subsequenceData[i] = new SubsequenceData(dbSubsequences[i], values, attributeIndexes);
                }

                // trying to save calculated characteristics to database
                var characteristicRepository = new CharacteristicRepository(context);
                characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

                return subsequenceData;
            }
        }
    }
}
