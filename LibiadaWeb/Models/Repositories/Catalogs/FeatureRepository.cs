namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Attributes;

    /// <summary>
    /// The feature repository.
    /// </summary>
    public static class FeatureRepository
    {
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
        public static Feature GetFeatureByName(string name)
        {
            return ArrayExtensions.ToArray<Feature>().Single(f => f.GetAttribute<Feature, GenBankFeatureNameAttribute>().Value == name);
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
        public static bool FeatureExists(string name)
        {
            return ArrayExtensions.ToArray<Feature>().Any(f => f.GetAttribute<Feature, GenBankFeatureNameAttribute>().Value == name);
        }
    }
}
