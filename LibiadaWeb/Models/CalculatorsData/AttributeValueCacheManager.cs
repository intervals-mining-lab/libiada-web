namespace LibiadaWeb.Models.CalculatorsData
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaWeb.Models.Repositories.Catalogs;

    public class AttributeValueCacheManager
    {
        public List<AttributeValue> AllAttributeValues => attributeValuesCache.ToList();

        /// <summary>
        /// United non-redundant list of all attributes values.
        /// </summary>
        private readonly List<AttributeValue> attributeValuesCache = new List<AttributeValue>();

        /// <summary>
        /// Dictionary of indexes of all attributeValues present in attributeValues list.
        /// </summary>
        private readonly Dictionary<AttributeValue, int> attributeValueIndexDictionary = new Dictionary<AttributeValue, int>();

        /// <summary>
        /// Retrieves attributes values of given subsequences from database
        /// and adds them to the united list.
        /// Also fills Attributes field of given <see cref="SubsequenceData"/> array.
        /// </summary>
        /// <param name="subsequencesData">
        /// subsequenceData array containing ids of corresponding subsequences.
        /// </param>
        public void FillAttributeValues(SubsequenceData[] subsequencesData)
        {
            long[] subsequenceIds = subsequencesData.Select(sd => sd.Id).ToArray();
            Dictionary<long, AttributeValue[]> subsequencesAttributes = GetSubsequencesAttributesValues(subsequenceIds);

            for (int i = 0; i < subsequencesData.Length; i++)
            {
                subsequencesAttributes.TryGetValue(subsequenceIds[i], out AttributeValue[] attributes);
                attributes = attributes ?? new AttributeValue[0];
                subsequencesData[i].Attributes = AddAttributeValues(attributes);
            }
        }

        /// <summary>
        /// Retrieves attributes values of given subsequences from database.
        /// </summary>
        /// <param name="subsequenceIds">
        /// Ids of subsequences which attributes values are to be retrieved.
        /// </param>
        /// <returns>
        /// Dictionary containing subsequences ids as keys and attributes values as values.
        /// </returns>
        private Dictionary<long, AttributeValue[]> GetSubsequencesAttributesValues(long[] subsequenceIds)
        {
            using (var db = new LibiadaWebEntities())
            {
                var sequenceAttributeRepository = new SequenceAttributeRepository(db);
                return sequenceAttributeRepository.GetAttributes(subsequenceIds);
            }
        }

        /// <summary>
        /// Adds attributes to united non-redundant list of all attributes values.
        /// </summary>
        /// <param name="values">
        /// The attributes values.
        /// </param>
        /// <returns>
        /// The indexes of added attributes values in list of all attributes values <see cref="T:int[]"/>.
        /// </returns>
        private int[] AddAttributeValues(AttributeValue[] values)
        {
            foreach (AttributeValue value in values)
            {
                if (!attributeValueIndexDictionary.ContainsKey(value))
                {
                    attributeValueIndexDictionary.Add(value, attributeValuesCache.Count);
                    attributeValuesCache.Add(value);
                }
            }

            return GetAttributeValueIndexesForValues(values);
        }

        /// <summary>
        /// Gets attributes indexes in list of all attributes values.
        /// </summary>
        /// <param name="values">
        /// The attribute values to search for.
        /// </param>
        /// <returns>
        /// The <see cref="T:int[]"/>.
        /// </returns>
        private int[] GetAttributeValueIndexesForValues(AttributeValue[] values)
        {
            return values.Select(v => attributeValueIndexDictionary[v]).ToArray();
        }
    }
}