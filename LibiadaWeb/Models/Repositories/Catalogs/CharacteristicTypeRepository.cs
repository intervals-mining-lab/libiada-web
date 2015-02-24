namespace LibiadaWeb.Models.Repositories.Catalogs
{
    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    public class CharacteristicTypeRepository : ICharacteristicTypeRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicTypeRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CharacteristicTypeRepository(LibiadaWebEntities db)
        {
            this.db = db;
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
