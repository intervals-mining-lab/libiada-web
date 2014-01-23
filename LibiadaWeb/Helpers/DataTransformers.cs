using System.Linq;
using System.Text;
using Npgsql;

namespace LibiadaWeb.Helpers
{
    public static class DataTransformers
    {
        public static string CleanFastaFile(string file)
        {
            string[] splittedFile = file.Split(new [] { '\0', '\t'});
            
            var result = new StringBuilder();

            foreach (string line in splittedFile)
            {
                result.Append(line);
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