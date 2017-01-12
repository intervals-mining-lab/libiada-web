namespace LibiadaWeb.Attributes
{
    using System;

    /// <summary>
    /// GenBank feature name attribute.
    /// Used to specify name of feature in geneBank standard.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GenBankFeatureNameAttribute : Attribute
    {
        /// <summary>
        /// Gets the genBank name of the feature.
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenBankFeatureNameAttribute"/> class.
        /// </summary>
        /// <param name="value">
        /// GenBank name of the feature.
        /// </param>
        public GenBankFeatureNameAttribute(string value)
        {
            Value = value;
        }
    }
}
