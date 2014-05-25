namespace LibiadaWeb.Math
{
    using System.Collections.Generic;

    using Clusterizator.Classes;

    /// <summary>
    /// Класс заполняющий таблицу данных для кластеризации
    /// </summary>
    public static class DataTableFiller
    {

        /// <summary>
        /// Метод, формирующий таблицу
        /// </summary>
        /// <param name="id">
        /// Массив номеров цепочек
        /// </param>
        /// <param name="characteristicsNames">
        /// Массив названий характеристик
        /// </param>
        /// <param name="characteristics">
        /// Двумерный массив характеристик
        /// </param>
        /// <returns>
        /// Таблица днных
        /// </returns>
        public static DataTable FillDataTable(long[] id, string[] characteristicsNames, List<List<double>> characteristics)
        {
            var tempTable = new DataTable();
            for (int j = 0; j < id.Length; j++)
            {
                // формируются строки и добавляются в таблицу
                tempTable.Add(FormDataObject(characteristics[j], characteristicsNames, id[j]));
            }

            return tempTable;
        }

        /// <summary>
        /// Метод ,формирующий строку таблицы
        /// </summary>
        /// <param name="characteristics">
        /// Строка таблицы характеристик
        /// </param>
        /// <param name="characteristicsNames">
        /// Массив названий характеристик
        /// </param>
        /// <param name="id">
        /// Номер цепочки
        /// </param>
        /// <returns>
        /// Строка таблицы
        /// </returns>
        private static DataObject FormDataObject(List<double> characteristics, string[] characteristicsNames, long id)
        {
            var tempObject = new DataObject {Id = id};
            for (int i = 0; i < characteristicsNames.Length; i++)
            {
                // добавляется очередное значение характеристики
                tempObject.Add(characteristicsNames[i], characteristics[i]);
            }

            return tempObject;
        }
    }
}