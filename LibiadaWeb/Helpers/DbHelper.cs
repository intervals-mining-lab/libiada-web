namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Npgsql;

    /// <summary>
    /// The helper for database functions.
    /// </summary>
    public static class DbHelper
    {
        /// <summary>
        /// Get data base name from data base.
        /// </summary>
        /// <param name="db">
        /// Data base connection.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> representing db name.
        /// </returns>
        public static string GetDbName(LibiadaWebEntities db)
        {
            string result;
            try
            {
                result = db.Database.SqlQuery<string>("SELECT current_database()").First();
            }
            catch (Exception e)
            {
                result = "Error: " + e.Message;
            }

            return result;
        }

        /// <summary>
        /// Gets new element id from data base.
        /// </summary>
        /// <param name="db">
        /// Data base connection.
        /// </param>
        /// <returns>
        /// The <see cref="long"/> value of new id.
        /// </returns>
        public static long GetNewElementId(LibiadaWebEntities db)
        {
            return db.Database.SqlQuery<long>("SELECT nextval('elements_id_seq');").First();
        }

        /// <summary>
        /// The get element ids.
        /// </summary>
        /// <param name="db">
        /// Data base connection.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Int64}"/>.
        /// </returns>
        public static List<long> GetElementIds(LibiadaWebEntities db, long sequenceId)
        {
            const string Query = "SELECT unnest(alphabet) FROM chain WHERE id = @id";
            return db.Database.SqlQuery<long>(Query, new NpgsqlParameter("@id", sequenceId)).ToList();
        }

        /// <summary>
        /// Gets building of sequence by id..
        /// </summary>
        /// <param name="db">
        /// Data base connection.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="int[]"/>.
        /// </returns>
        public static int[] GetBuilding(LibiadaWebEntities db, long sequenceId)
        {
            const string Query = "SELECT unnest(building) FROM chain WHERE id = @id";
            return db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", sequenceId)).ToArray();
        }

        public static void ExecuteCommand(LibiadaWebEntities db, string query, object[] parameters)
        {
            db.Database.ExecuteSqlCommand(query, parameters);
        }
    }
}
