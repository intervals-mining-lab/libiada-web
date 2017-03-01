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
        /// <param name="characteristicTypeLinkIds">
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
            short[] characteristicTypeLinkIds,
            Feature[] features,
            long parentSequenceId,
            List<AttributeValue> attributeValues,
            string[] filters = null)
        {
            // creating local context to avoid memory overflow due to possibly big cache of characteristics
            using (var context = new LibiadaWebEntities())
            {
                var subsequenceExtractor = new SubsequenceExtractor(context);
                var sequenceAttributeRepository = new SequenceAttributeRepository(context);
                var newCharacteristics = new List<CharacteristicValue>();
                var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];
                var links = new Link[characteristicTypeLinkIds.Length];

                // extracting data from database
                var dbSubsequences = filters == null ? subsequenceExtractor.GetSubsequences(parentSequenceId, features) : subsequenceExtractor.GetSubsequences(parentSequenceId, features, filters);
                var subsequenceIds = dbSubsequences.Select(s => s.Id).ToArray();
                var dbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);

                var dbCharacteristics = context.CharacteristicValue.Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && subsequenceIds.Contains(c.SequenceId))
                        .ToArray()
                        .GroupBy(c => c.SequenceId)
                        .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

                var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(context);
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    var characteristicTypeLinkId = characteristicTypeLinkIds[k];
                    string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                    calculators[k] = FullCalculatorsFactory.CreateFullCalculator(className);
                    links[k] = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                }

                // converting to libiada sequences
                var sequences = subsequenceExtractor.ExtractChains(dbSubsequences);
                var subsequenceData = new SubsequenceData[sequences.Length];

                // cycle through subsequences
                for (int i = 0; i < sequences.Length; i++)
                {
                    var values = new double[characteristicTypeLinkIds.Length];
                    Dictionary<short, double> sequenceDbCharacteristics;
                    if (!dbCharacteristics.TryGetValue(dbSubsequences[i].Id, out sequenceDbCharacteristics))
                    {
                        sequenceDbCharacteristics = new Dictionary<short, double>();
                    }

                    // cycle through characteristics and notations
                    for (int j = 0; j < characteristicTypeLinkIds.Length; j++)
                    {
                        short characteristicTypeLinkId = characteristicTypeLinkIds[j];
                        if (!sequenceDbCharacteristics.TryGetValue(characteristicTypeLinkId, out values[j]))
                        {
                            values[j] = calculators[j].Calculate(sequences[i], links[j]);
                            var currentCharacteristic = new CharacteristicValue
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
