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
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id, 
                Text = n.name, 
                Selected = false, 
                Nature = n.nature_id
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedDb">
        /// The selected db.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(int selectedDb)
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id, 
                Text = n.name, 
                Selected = n.id == selectedDb, 
                Nature = n.nature_id
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedDbs">
        /// The selected dbs.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(List<int> selectedDbs)
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id, 
                Text = n.name, 
                Selected = selectedDbs.Contains(n.id), 
                Nature = n.nature_id
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
