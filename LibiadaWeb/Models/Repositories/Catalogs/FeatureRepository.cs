namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaWeb.Attributes;
    using LibiadaWeb.Extensions;
    using LibiadaWeb.Models.CalculatorsData;

    /// <summary>
    /// The feature repository.
    /// </summary>
    public class FeatureRepository
    {
        /// <summary>
        /// Gets the features.
        /// </summary>
        public IEnumerable<Feature> Features
        {
            get
            {
                return EnumExtensions.ToArray<Feature>();
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Gets feature by name.
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
        public Feature GetFeatureByName(string name)
        {
            return Features.Single(f => f.GetAttribute<Feature, GenBankFeatureNameAttribute>().Value == name);
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
            return Features.Any(f => f.GetAttribute<Feature, GenBankFeatureNameAttribute>().Value == name);
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature()
        {
            return GetSelectListWithNature(new Feature[0]);
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
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature(Feature selectedFeature)
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
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature(IEnumerable<Feature> selectedFeatures)
        {
            return GetSelectListWithNature(Features, selectedFeatures);
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="selectedFeatures">
        /// The selected features.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature(IEnumerable<Feature> features, IEnumerable<Feature> selectedFeatures)
        {
            return features.Select(p => new SelectListItemWithNature
            {
                Value = ((byte)p).ToString(),
                Text = p.GetDisplayValue(),
                Selected = selectedFeatures.Contains(p),
                Nature = (byte)p.GetNature()
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <param name="selectedFeature">
        /// The selected Feature.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<SelectListItemWithNature> GetSelectListWithNature(IEnumerable<Feature> features, Feature selectedFeature)
        {
            return GetSelectListWithNature(features, new List<Feature> { selectedFeature });
        }
    }
}
