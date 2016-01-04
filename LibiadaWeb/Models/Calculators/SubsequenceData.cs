namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The genes data.
    /// </summary>
    public class SubsequenceData
    {
        /// <summary>
        /// The starts.
        /// </summary>
        public readonly List<int> Starts;

        /// <summary>
        /// The lengths.
        /// </summary>
        public readonly List<int> Lengths;

        /// <summary>
        /// The feature id.
        /// </summary>
        public readonly int FeatureId;

        /// <summary>
        /// Subsequence feature name.
        /// </summary>
        public readonly string FeatureName;

        /// <summary>
        /// The attributes.
        /// </summary>
        public readonly List<string> Attributes;

        /// <summary>
        /// The web api id.
        /// </summary>
        public readonly int? WebApiId;

        /// <summary>
        /// The characteristic.
        /// </summary>
        public readonly double Characteristic;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceData"/> class.
        /// </summary>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        /// <param name="characteristic">
        /// The characteristic.
        /// </param>
        /// <param name="attributes">
        /// The attributes.
        /// </param>
        public SubsequenceData(Subsequence subsequence, double characteristic, List<string> attributes)
        {
            WebApiId = subsequence.WebApiId;
            Starts = new List<int> { subsequence.Start };
            Lengths = new List<int> { subsequence.Length };
            foreach (var position in subsequence.Position)
            {
                Starts.Add(position.Start);
                Lengths.Add(position.Length);
            }

            FeatureId = subsequence.FeatureId;
            FeatureName = subsequence.Feature.Name;
            Attributes = subsequence.SequenceAttribute.Select(a => a.Attribute.Name + " = " + a.Value).ToList();

            Characteristic = characteristic;
            Attributes = attributes;
        }
    }
}
