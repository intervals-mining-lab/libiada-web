namespace LibiadaWeb.Models.Calculators
{
    /// <summary>
    /// The genes characteristics.
    /// </summary>
    public class GeneCharacteristic
    {
        /// <summary>
        /// The gene.
        /// </summary>
        public readonly Gene Gene;

        /// <summary>
        /// The characteristic.
        /// </summary>
        public readonly double Characteristic;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneCharacteristic"/> class.
        /// </summary>
        /// <param name="gene">
        /// The gene.
        /// </param>
        /// <param name="characteristic">
        /// The characteristic.
        /// </param>
        public GeneCharacteristic(Gene gene, double characteristic)
        {
            this.Gene = gene;
            this.Characteristic = characteristic;
        }
    }
}