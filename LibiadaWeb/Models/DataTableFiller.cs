using System.Collections;
using System.Collections.Generic;
using NewClusterization.Classes.DataMining.Clusterization;

namespace LibiadaWeb.Models
{
    /// <summary>
    /// Класс заполняющий таблицу данных для кластеризации
    /// </summary>
    public static class DataTableFiller
    {

        /// <summary>
        /// Метод, формирующий таблицу
        /// </summary>
        /// <param name="id">Массив номеров цепочек</param>
        /// <param name="characteristicsNames">Массив названий характеристик</param>
        /// <param name="characteristics">Двумерный массив характеристик</param>
        /// <returns>Таблица днных</returns>
        public static DataTable FillDataTable(long[] id, string[] characteristicsNames, List<List<double>> characteristics)
        {
            DataTable TempTable = new DataTable();
            for (int j = 0; j < id.Length; j++)
            {
                //формируются строки и добавляются в таблицу
                TempTable.Add(FormDataObject(characteristics[j], characteristicsNames, id[j]));
            }

            return TempTable;
        }

        /// <summary>
        /// Метод ,формирующий строку таблицы
        /// </summary>
        /// <param name="characteristics">Строка таблицы характеристик</param>
        /// <param name="characteristicsNames">Массив названий характеристик</param>
        /// <param name="id">Номер цепочки</param>
        /// <returns>Строка таблицы</returns>
        public static DataObject FormDataObject(List<double> characteristics, string[] characteristicsNames, long id)
        {
            DataObject TempObject = new DataObject();
            TempObject.Id = id;
            for (int i = 0; i < characteristicsNames.Length; i++)
            {
                //добавляется очередное значение характеристики
                TempObject.Add(characteristicsNames[i], characteristics[i]);
            }
            return TempObject;
        }
    }
}