namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Extensions;

    using FeatureExtensions = LibiadaWeb.Extensions.FeatureExtensions;

    /// <summary>
    /// The feature repository.
    /// </summary>
    public static class FeatureRepository
    {
        private static readonly Dictionary<string, Feature> featuresDictionary = EnumExtensions.ToArray<Feature>()
                                                                                               .ToDictionary(FeatureExtensions.GetGenBankName);

        /// <summary>
        /// Gets feature by name.
        /// </summary>
        /// <param name="genBankName">
        /// The feature name in GenBank.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static Feature GetFeatureByName(string genBankName)
        {
            return featuresDictionary[genBankName];
        }

        /// <summary>
        /// Checks if feature exists.
        /// </summary>
        /// <param name="genBankName">
        /// The feature name in GenBank.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool FeatureExists(string genBankName)
        {
            return featuresDictionary.ContainsKey(genBankName);
        }
    }
}
