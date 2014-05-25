// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The helper for database functions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibiadaWeb.Helpers
{
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
            return db.Database.SqlQuery<string>("SELECT current_database()").First();
        }    
    }
}