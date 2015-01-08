namespace LibiadaWeb.Helpers
{
    using System;
    using System.Linq;

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
    }
}
