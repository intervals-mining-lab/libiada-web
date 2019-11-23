namespace LibiadaWeb.Extensions
{
    using LibiadaCore.Extensions;

    using LibiadaWeb.Attributes;

    /// <summary>
    /// Feature extension methods.
    /// </summary>
    public static class FeatureExtensions
    {
        /// <summary>
        /// Gets genBank feature name.
        /// </summary>
        /// <param name="value">
        /// Feature value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetGenBankName(this Feature value)
        {
            return value.GetAttribute<Feature, GenBankFeatureNameAttribute>().Value;
        }
    }
}