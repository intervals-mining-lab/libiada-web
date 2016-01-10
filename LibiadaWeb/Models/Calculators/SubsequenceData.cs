namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;

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
        /// The attributes.
        /// </summary>
        public readonly string[] Attributes;

        /// <summary>
        /// The web api id.
        /// </summary>
        public readonly int? WebApiId;

        /// <summary>
        /// The characteristics.
        /// </summary>
        public readonly double[] CharacteristicsValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceData"/> class.
        /// </summary>
        /// <param name="subsequence">
        /// The subsequence.
        /// </param>
        /// <param name="characteristics">
        /// The characteristic.
        /// </param>
        /// <param name="attributes">
        /// The attributes.
        /// </param>
        public SubsequenceData(Subsequence subsequence, double[] characteristics, string[] attributes)
        {
            CharacteristicsValues = characteristics;
            Attributes = attributes ?? new string[0];
            FeatureId = subsequence.FeatureId;
            WebApiId = subsequence.WebApiId;

            Starts = new List<int> { subsequence.Start };
            Lengths = new List<int> { subsequence.Length };
            foreach (var position in subsequence.Position)
            {
                Starts.Add(position.Start);
                Lengths.Add(position.Length);
            }
        }
    }
}
