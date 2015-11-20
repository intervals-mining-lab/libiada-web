namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The genes characteristics.
    /// </summary>
    public class SubsequenceCharacteristic
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
        /// The feature.
        /// </summary>
        public readonly string Feature; 

        /// <summary>
        /// The characteristic.
        /// </summary>
        public readonly double Characteristic;

        /// <summary>
        /// The attributes.
        /// </summary>
        public readonly List<string> Attributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceCharacteristic"/> class.
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
        public SubsequenceCharacteristic(Subsequence subsequence, double characteristic, List<string> attributes)
        {
            Starts = new List<int>();
            Starts.Add(subsequence.Start);
            Lengths = new List<int>();
            Lengths.Add(subsequence.Length);
            foreach (var position in subsequence.Position)
            {
                Starts.Add(position.Start);
                Lengths.Add(position.Length);
            }

            Feature = subsequence.Feature.Name;
            Attributes = subsequence.SequenceAttribute.Select(a => a.Attribute.Name + " = " + a.Value).ToList();

            Characteristic = characteristic;
            Attributes = attributes;
        }
    }
}
