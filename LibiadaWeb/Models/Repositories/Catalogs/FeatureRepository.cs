namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using FeatureExtensions = LibiadaWeb.Extensions.FeatureExtensions;

    /// <summary>
    /// The feature repository.
    /// </summary>
    public static class FeatureRepository
    {
        private static readonly Feature[] features = EnumExtensions.ToArray<Feature>();

        /// <summary>
        /// Gets feature by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static Feature GetFeatureByName(string name)
        {
            return features.Single(f => FeatureExtensions.GetGenBankName(f) == name);
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
            return features.Any(f => FeatureExtensions.GetGenBankName(f) == name);
        }
    }
}
