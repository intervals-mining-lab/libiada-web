namespace LibiadaWeb.Models.Calculators
{
    /// <summary>
    /// The genes characteristics.
    /// </summary>
    public class FragmentCharacteristic
    {
        /// <summary>
        /// The gene.
        /// </summary>
        public readonly Fragment Fragment;

        /// <summary>
        /// The characteristic.
        /// </summary>
        public readonly double Characteristic;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneCharacteristic"/> class.
        /// </summary>
        /// <param name="fragment">
        /// The gene.
        /// </param>
        /// <param name="characteristic">
        /// The characteristic.
        /// </param>
        public FragmentCharacteristic(Fragment fragment, double characteristic)
        {
            this.Fragment = fragment;
            this.Characteristic = characteristic;
        }
    }
}