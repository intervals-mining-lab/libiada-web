namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;

    /// <summary>
    /// The genes characteristics.
    /// </summary>
    public class SubsequenceCharacteristic
    {
        /// <summary>
        /// The subsequence.
        /// </summary>
        public readonly Subsequence Subsequence;

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
            Subsequence = subsequence;
            Characteristic = characteristic;
            Attributes = attributes;
        }
    }
}
