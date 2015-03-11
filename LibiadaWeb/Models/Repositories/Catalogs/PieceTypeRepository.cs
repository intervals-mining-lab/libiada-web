namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The piece type repository.
    /// </summary>
    public class FeatureRepository : IFeatureRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public FeatureRepository(LibiadaWebEntities db)
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

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.Feature.Select(p => new
            {
                Value = p.Id, 
                Text = p.Name, 
                Selected = false, 
                Nature = p.NatureId
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedFeature">
        /// The selected piece type.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(int selectedFeature)
        {
            return db.Feature.Select(p => new
            {
                Value = p.Id, 
                Text = p.Name, 
                Selected = p.Id == selectedFeature, 
                Nature = p.NatureId
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedFeatures">
        /// The selected piece types.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedFeatures)
        {
            return db.Feature.Select(p => new
            {
                Value = p.Id, 
                Text = p.Name, 
                Selected = selectedFeatures.Contains(p.Id), 
                Nature = p.NatureId
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedFeatures">
        /// The selected piece types.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedFeatures, IEnumerable<int> filter)
        {
            return db.Feature.Where(p => filter.Contains(p.Id)).Select(p => new
            {
                Value = p.Id, 
                Text = p.Name, 
                Selected = selectedFeatures.Contains(p.Id), 
                Nature = p.NatureId
            });
        }
    }
}
