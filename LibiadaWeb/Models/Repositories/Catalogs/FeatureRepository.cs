namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The feature repository.
    /// </summary>
    public class FeatureRepository : IFeatureRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The features.
        /// </summary>
        private readonly List<Feature> features;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public FeatureRepository(LibiadaWebEntities db)
        {
            this.db = db;
            features = db.Feature.ToList();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose() 
        {
            db.Dispose();
        }

        /// <summary>
        /// Gets feature id by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if feature type is unknown.
        /// </exception>
        public int GetFeatureIdByName(string name)
        {
            return features.Single(f => f.Type == name).Id;
        }

        /// <summary>
        /// Gets feature name by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetFeatureNameById(int id)
        {
            return features.Single(f => f.Id == id).Name;
        } 

        /// <summary>
        /// Checks if feature exists.
        /// </summary>
        /// <param name="name">
        /// The name of feature.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool FeatureExists(string name)
        {
            return features.Any(f => f.Type == name);
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return GetSelectListWithNature(new int[0]);
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedFeature">
        /// The selected feature.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(int selectedFeature)
        {
            return GetSelectListWithNature(new[] { selectedFeature });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedFeatures">
        /// The selected features.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedFeatures)
        {
            return GetSelectListWithNature(selectedFeatures, db.Feature.Select(f => f.Id));
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedFeatures">
        /// The selected features.
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
