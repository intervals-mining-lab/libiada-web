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
            features = db.Feature.OrderBy(f => f.Id).ToList();
        }

        /// <summary>
        /// Gets the features.
        /// </summary>
        public IEnumerable<Feature> Features
        {
            get
            {
                return features.ToList();
            }
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
        /// The get features by id.
        /// </summary>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Feature}"/>.
        /// </returns>
        public IEnumerable<Feature> GetFeaturesById(int[] featureIds)
        {
            return features.Where(f => featureIds.Contains(f.Id));
        }

        /// <summary>
        /// The get feature by id.
        /// </summary>
        /// <param name="featureId">
        /// The feature id.
        /// </param>
        /// <returns>
        /// The <see cref="Feature"/>.
        /// </returns>
        public Feature GetFeatureById(int featureId)
        {
            return features.Single(f => f.Id == featureId);
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
            return GetSelectListWithNature(features.Select(f => f.Id), selectedFeatures);
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="featureIds">
        /// The features.
        /// </param>
        /// <param name="selectedFeatures">
        /// The selected features.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> featureIds, IEnumerable<int> selectedFeatures)
        {
            return features.Where(p => featureIds.Contains(p.Id)).Select(p => new
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
        /// <param name="featureIds">
        /// The features.
        /// </param>
        /// <param name="selectedFeature">
        /// The selected Feature.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> featureIds, int selectedFeature)
        {
            return GetSelectListWithNature(featureIds, new List<int> { selectedFeature });
        }
    }
}
