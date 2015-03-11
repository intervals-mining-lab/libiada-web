namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;

    /// <summary>
    /// The sequence characteristics.
    /// </summary>
    public class SequenceCharacteristics
    {
        /// <summary>
        /// The matter name.
        /// </summary>
        public readonly string MatterName;

        /// <summary>
        /// The characteristic.
        /// </summary>
        public readonly double Characteristic;

        /// <summary>
        /// The genes characteristics.
        /// </summary>
        public readonly List<FragmentCharacteristic> FragmentsCharacteristics;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceCharacteristics"/> class.
        /// </summary>
        /// <param name="matterName">
        /// The matter name.
        /// </param>
        /// <param name="characteristic">
        /// The characteristic.
        /// </param>
        /// <param name="fragmentsCharacteristics">
        /// The genes characteristics.
        /// </param>
        public SequenceCharacteristics(string matterName, double characteristic, List<FragmentCharacteristic> fragmentsCharacteristics)
        {
            this.MatterName = matterName;
            this.Characteristic = characteristic;
            this.FragmentsCharacteristics = fragmentsCharacteristics;
        }
    }
}