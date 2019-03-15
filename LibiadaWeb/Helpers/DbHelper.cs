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
                        databaseName = $"No connection to db. Reason: {e.Message}";
                    }
                }

                return databaseName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether connection to db established or not.
        /// </summary>
        public static bool ConnectionSuccessful
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
        /// Gets building of sequence by id.
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
        /// Gets fmotif's alphabet ids.
        /// </summary>
        /// <param name="db">
        /// Database connection.
        /// </param>
        /// <param name="fmotifId">
        /// The fmotif id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Int64}"/>.
        /// </returns>
        public static List<long> GetFmotifAlphabet(LibiadaWebEntities db, long fmotifId)
        {
            const string Query = "SELECT unnest(alphabet) FROM fmotif WHERE id = @id";
            return db.Database.SqlQuery<long>(Query, new NpgsqlParameter("@id", fmotifId)).ToList();
        }

        /// <summary>
        /// Gets building of fmotif by id.
        /// </summary>
        /// <param name="db">
        /// Database connection.
        /// </param>
        /// <param name="fmotifId">
        /// The fmotif id.
        /// </param>
        /// <returns>
        /// The <see cref="T:int[]"/>.
        /// </returns>
        public static int[] GetFmotifBuilding(LibiadaWebEntities db, long fmotifId)
        {
            const string Query = "SELECT unnest(building) FROM fmotif WHERE id = @id";
            return db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", fmotifId)).ToArray();
        }

        /// <summary>
        /// Gets measure's alphabet ids.
        /// </summary>
        /// <param name="db">
        /// Database connection.
        /// </param>
        /// <param name="measureId">
        /// The fmotif id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Int64}"/>.
        /// </returns>
        public static List<long> GetMeasureAlphabet(LibiadaWebEntities db, long measureId)
        {
            const string Query = "SELECT unnest(alphabet) FROM measure WHERE id = @id";
            return db.Database.SqlQuery<long>(Query, new NpgsqlParameter("@id", measureId)).ToList();
        }

        /// <summary>
        /// Gets building of measure by id.
        /// </summary>
        /// <param name="db">
        /// Database connection.
        /// </param>
        /// <param name="measureId">
        /// The fmotif id.
        /// </param>
        /// <returns>
        /// The <see cref="T:int[]"/>.
        /// </returns>
        public static int[] GetMeasureBuilding(LibiadaWebEntities db, long measureId)
        {
            const string Query = "SELECT unnest(building) FROM measure WHERE id = @id";
            return db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", measureId)).ToArray();
        }

        /// <summary>
        /// Gets fmotif's alphabet ids.
        /// </summary>
        /// <param name="db">
        /// Database connection.
        /// </param>
        /// <param name="musicChainId">
        /// The fmotif id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Int64}"/>.
        /// </returns>
        public static List<long> GetMusicChainAlphabet(LibiadaWebEntities db, long musicChainId)
        {
            const string Query = "SELECT unnest(alphabet) FROM music_chain WHERE id = @id";
            return db.Database.SqlQuery<long>(Query, new NpgsqlParameter("@id", musicChainId)).ToList();
        }

        /// <summary>
        /// Gets building of fmotif by id.
        /// </summary>
        /// <param name="db">
        /// Database connection.
        /// </param>
        /// <param name="musicChainId">
        /// The fmotif id.
        /// </param>
        /// <returns>
        /// The <see cref="T:int[]"/>.
        /// </returns>
        public static int[] GetMusicChainBuilding(LibiadaWebEntities db, long musicChainId)
        {
            const string Query = "SELECT unnest(building) FROM music_chain WHERE id = @id";
            return db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", musicChainId)).ToArray();
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

        /// <summary>
        /// Extracts sequence length from database.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetSequenceLength(LibiadaWebEntities db, long sequenceId)
        {
            const string Query = "SELECT array_length(building, 1) FROM chain WHERE id = @id";
            return db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", sequenceId)).First();
        }
    }
}
