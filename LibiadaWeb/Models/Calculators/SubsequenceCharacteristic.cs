namespace LibiadaWeb.Models.Calculators
{
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
        /// Initializes a new instance of the <see cref="SubsequenceCharacteristic"/> class.
        /// </summary>
        /// <param name="subsequence">
        /// The gene.
        /// </param>
        /// <param name="characteristic">
        /// The characteristic.
        /// </param>
        public SubsequenceCharacteristic(Subsequence subsequence, double characteristic)
        {
            this.Subsequence = subsequence;
            this.Characteristic = characteristic;
        }
    }
}