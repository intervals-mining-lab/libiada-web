namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The genes data.
    /// </summary>
    public struct SubsequenceData
    {
        /// <summary>
        /// The starts.
        /// </summary>
        public readonly int[] Starts;

        /// <summary>
        /// The lengths.
        /// </summary>
        public readonly int[] Lengths;

        /// <summary>
        /// The feature id.
        /// </summary>
        public readonly int FeatureId;

        /// <summary>
        /// The attributes ids array.
        /// </summary>
        public readonly int[] Attributes;

        /// <summary>
        /// The partial flag.
        /// </summary>
        public readonly bool Partial;

        /// <summary>
        /// Sequence remote id.
        /// </summary>
        public readonly string RemoteId;

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
        /// Attributes of the given subsequence in form of dictionary.
        /// </param>
        public SubsequenceData(Subsequence subsequence, double[] characteristics, int[] attributes)
        {
            CharacteristicsValues = characteristics;
            Attributes = attributes ?? new int[0];
            FeatureId = subsequence.FeatureId;
            RemoteId = subsequence.RemoteId;
            Partial = subsequence.Partial;

            var positions = subsequence.Position.ToArray();

            Starts = new int[positions.Length + 1];
            Starts[0] = subsequence.Start;
            Lengths = new int[positions.Length + 1];
            Lengths[0] = subsequence.Length;
            for (int i = 0; i < positions.Length; i++)
            {
                Starts[i + 1] = positions[i].Start;
                Lengths[i + 1] = positions[i].Length;
            }
        }
    }
}
