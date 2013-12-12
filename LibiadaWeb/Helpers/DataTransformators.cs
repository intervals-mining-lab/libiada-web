using System.Linq;
using System.Text;
using System.Web.Mvc;
using Npgsql;

namespace LibiadaWeb.Helpers
{
    public static class DataTransformators
    {
        public static string CleanFastaFile(string file)
        {
            string[] splittedFile = file.Split(new [] { '\0', '\t'});
            
            StringBuilder result = new StringBuilder();

            for (int k = 0; k < splittedFile.Length; k++)
            {
                result.Append(splittedFile[k]);
            }

            return result.ToString();
        }

        public static long GetLongSequenceValue(LibiadaWebEntities db, string name)
        {
            //TODO: сделать нормальный вызов функции. Проверить, нужна ли @ в имени параметра
            object[] param = { new NpgsqlParameter("@name", name) };
            return db.ExecuteStoreQuery<long>("SELECT seq_next_value(@name);", param).First();
        }

        public static int GetIntSequenceValue(LibiadaWebEntities db, string name)
        {
            return (int)GetLongSequenceValue(db, name);
        }
    }
}