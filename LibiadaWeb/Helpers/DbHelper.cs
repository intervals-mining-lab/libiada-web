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
        /// The database name.
        /// </summary>
        private static string databaseName;

        /// <summary>
        /// Gets the db name.
        /// </summary>
        public static string DbName
        {
            get
            {
                if (string.IsNullOrEmpty(databaseName))
                {
                    try
                    {
                        using (var db = new LibiadaWebEntities())
                        {
                            databaseName = string.Join("@", db.Database.Connection.DataSource, db.Database.Connection.Database);
                        }
                    }
                    catch (Exception e)
                    {
                        databaseName = "No connection to db. Reason: " + e.Message;
                    }
                }

                return databaseName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether connection to db established or not.
        /// </summary>
        public static bool ConnectionStatus
        {
            get
            {
                using (var db = new LibiadaWebEntities())
                {
                    return db.Database.Exists();
                }
            }
        }

        /// <summary>
        /// Gets new element id from database.
        /// </summary>
        /// <param name="db">
        /// Database connection.
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
        /// Database connection.
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
        /// Database connection.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="T:int[]"/>.
        /// </returns>
        public static int[] GetBuilding(LibiadaWebEntities db, long sequenceId)
        {
            const string Query = "SELECT unnest(building) FROM chain WHERE id = @id";
            return db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", sequenceId)).ToArray();
        }

        /// <summary>
        /// The execute custom sql command with parameters.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        public static void ExecuteCommand(LibiadaWebEntities db, string query, object[] parameters)
        {
            db.Database.ExecuteSqlCommand(query, parameters);
        }
    }
}
