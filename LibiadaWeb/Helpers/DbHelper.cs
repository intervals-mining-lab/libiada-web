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
        /// The connection.
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
    }
}