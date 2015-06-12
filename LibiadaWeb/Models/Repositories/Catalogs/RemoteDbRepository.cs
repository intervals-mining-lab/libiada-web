namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The remote db repository.
    /// </summary>
    public class RemoteDbRepository : IRemoteDbRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteDbRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public RemoteDbRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.RemoteDb.Select(n => new
            {
                Value = n.Id, 
                Text = n.Name, 
                Selected = false, 
                Nature = n.NatureId
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedDb">
        /// The selected db.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(int selectedDb)
        {
            return db.RemoteDb.Select(n => new
            {
                Value = n.Id, 
                Text = n.Name, 
                Selected = n.Id == selectedDb, 
                Nature = n.NatureId
            });
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose() 
        {
            db.Dispose();
        }
    }
}
