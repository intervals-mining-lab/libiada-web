namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Linq;

    using LibiadaCore.Extensions;

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
        public static Feature GetFeatureByName(string name)
        {
            return EnumExtensions.ToArray<Feature>().Single(f => f.GetGenBankName() == name);
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
            return EnumExtensions.ToArray<Feature>().Any(f => f.GetGenBankName() == name);
        }
    }
}
