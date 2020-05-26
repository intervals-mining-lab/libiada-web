namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Npgsql;
    using NpgsqlTypes;

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
                try
                {
                    using (var db = new LibiadaWebEntities())
                    {
                        return db.Database.Exists();
                    }
                }
                catch
                {
                    return false;
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
        public static long GetNewElementId(this LibiadaWebEntities db)
        {
            return db.Database.SqlQuery<long>("SELECT nextval('elements_id_seq');").Single();
        }

        /// <summary>
        /// TExtracts alphabet elements ids for given sequence.
        /// </summary>
        /// <param name="db">
        /// Database connection.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="long[]"/>.
        /// </returns>
        public static long[] GetAlphabetElementIds(this LibiadaWebEntities db, long sequenceId)
        {
            var dbConnection = (NpgsqlConnection)db.Database.Connection;
            const string Query = "SELECT alphabet FROM chain WHERE id = @id";
            var id = new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = sequenceId };
            long[] result;
            dbConnection.Open();
            using (var command = new NpgsqlCommand(Query, dbConnection))
            {
                command.Parameters.Add(id);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        dbConnection.Close();
                        throw new ArgumentException($"sequence id {sequenceId} does not exist", nameof(sequenceId));
                    }

                    result = (long[])reader[0];
                }
            }
            dbConnection.Close();
            return result;
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
        public static int[] GetSequenceBuilding(this LibiadaWebEntities db, long sequenceId)
        {
            var dbConnection = (NpgsqlConnection)db.Database.Connection;
            const string Query = "SELECT building FROM chain WHERE id = @id";
            var id = new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = sequenceId };
            int[] result;
            dbConnection.Open();
            using (var command = new NpgsqlCommand(Query, dbConnection))
            {
                command.Parameters.Add(id);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        dbConnection.Close();
                        throw new ArgumentException($"sequence id {sequenceId} does not exist", nameof(sequenceId));
                    }

                    result = (int[])reader[0];
                }
            }
            dbConnection.Close();
            return result;
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
        public static long[] GetFmotifAlphabet(this LibiadaWebEntities db, long fmotifId)
        {
            var dbConnection = (NpgsqlConnection)db.Database.Connection;
            const string Query = "SELECT alphabet FROM fmotif WHERE id = @id";
            var id = new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = fmotifId };
            long[] result;
            dbConnection.Open();
            using (var command = new NpgsqlCommand(Query, dbConnection))
            {
                command.Parameters.Add(id);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        dbConnection.Close();
                        throw new ArgumentException($"sequence id {fmotifId} does not exist", nameof(fmotifId));
                    }

                    result = (long[])reader[0];
                }
            }
            dbConnection.Close();
            return result;
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
        public static int[] GetFmotifBuilding(this LibiadaWebEntities db, long fmotifId)
        {
            var dbConnection = (NpgsqlConnection)db.Database.Connection;
            const string Query = "SELECT building FROM fmotif WHERE id = @id";
            var id = new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = fmotifId };
            int[] result;
            dbConnection.Open();
            using (var command = new NpgsqlCommand(Query, dbConnection))
            {
                command.Parameters.Add(id);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        dbConnection.Close();
                        throw new ArgumentException($"sequence id {fmotifId} does not exist", nameof(fmotifId));
                    }

                    result = (int[])reader[0];
                }
            }
            dbConnection.Close();
            return result;
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
        public static long[] GetMeasureAlphabet(this LibiadaWebEntities db, long measureId)
        {
            var dbConnection = (NpgsqlConnection)db.Database.Connection;
            const string Query = "SELECT alphabet FROM measure WHERE id = @id";
            var id = new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = measureId };
            long[] result;
            dbConnection.Open();
            using (var command = new NpgsqlCommand(Query, dbConnection))
            {
                command.Parameters.Add(id);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        dbConnection.Close();
                        throw new ArgumentException($"sequence id {measureId} does not exist", nameof(measureId));
                    }

                    result = (long[])reader[0];
                }
            }
            dbConnection.Close();
            return result;
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
        public static int[] GetMeasureBuilding(this LibiadaWebEntities db, long measureId)
        {
            var dbConnection = (NpgsqlConnection)db.Database.Connection;
            const string Query = "SELECT building FROM measure WHERE id = @id";
            var id = new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = measureId };
            int[] result;
            dbConnection.Open();
            using (var command = new NpgsqlCommand(Query, dbConnection))
            {
                command.Parameters.Add(id);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        dbConnection.Close();
                        throw new ArgumentException($"sequence id {measureId} does not exist", nameof(measureId));
                    }

                    result = (int[])reader[0];
                }
            }
            dbConnection.Close();
            return result;
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
        public static void ExecuteCommand(this LibiadaWebEntities db, string query, params NpgsqlParameter[] parameters)
        {
            // TODO: check if this is the optimal way
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
        public static int GetSequenceLength(this LibiadaWebEntities db, long sequenceId)
        {
            const string Query = "SELECT cardinality(building) FROM chain WHERE id = @id";
            var id = new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = sequenceId };
            return db.Database.SqlQuery<int>(Query, id).First();
        }
    }
}
