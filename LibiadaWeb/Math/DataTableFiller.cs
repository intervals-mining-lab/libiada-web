namespace LibiadaWeb.Math
{
    using System.Collections.Generic;

    using Clusterizator;

    /// <summary>
    /// Class filling results table of clusterization
    /// </summary>
    public static class DataTableFiller
    {
        /// <summary>
        /// Fills data table.
        /// </summary>
        /// <param name="id">
        /// Array of sequences ids.
        /// </param>
        /// <param name="characteristicsNames">
        /// Array of characteristics names.
        /// </param>
        /// <param name="characteristics">
        /// Two-dimensional array of characteristics values.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable FillDataTable(long[] id, string[] characteristicsNames, List<List<double>> characteristics)
        {
            var result = new DataTable();
            for (int j = 0; j < id.Length; j++)
            {
                result.Add(FormDataObject(characteristics[j], characteristicsNames, id[j]));
            }

            return result;
        }

        /// <summary>
        /// Fills row of data table.
        /// </summary>
        /// <param name="characteristics">
        /// Row of characteristics table.
        /// </param>
        /// <param name="characteristicsNames">
        /// Characteristics names array.
        /// </param>
        /// <param name="id">
        /// Sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="DataObject"/>.
        /// </returns>
        private static DataObject FormDataObject(List<double> characteristics, string[] characteristicsNames, long id)
        {
            var result = new DataObject { Id = id };
            for (int i = 0; i < characteristicsNames.Length; i++)
            {
                result.Add(characteristicsNames[i], characteristics[i]);
            }

            return result;
        }
    }
}
