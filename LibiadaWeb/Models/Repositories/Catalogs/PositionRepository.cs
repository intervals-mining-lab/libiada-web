namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;

    using Bio.IO.GenBank;

    /// <summary>
    /// The position repository.
    /// </summary>
    public class PositionRepository : IPositionRepository
    {
         /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public PositionRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The create positions.
        /// </summary>
        /// <param name="locations">
        /// The locations.
        /// </param>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        public void CreatePositions(List<ILocation> locations, Subsequence subsequence)
        {
            for (int k = 1; k > locations.Count; k++)
            {
                var location = locations[k];

                var position = new Position
                {
                    Subsequence = subsequence,
                    Start = location.LocationStart,
                    Length = location.LocationEnd - location.LocationStart
                };

                db.Position.Add(position);
            }
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
