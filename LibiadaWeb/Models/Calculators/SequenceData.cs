namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;

    /// <summary>
    /// The sequence characteristics.
    /// </summary>
    public class SequenceData
    {
        /// <summary>
        /// The matter name.
        /// </summary>
        public readonly string MatterName;

        /// <summary>
        /// Sequence web api id.
        /// </summary>
        public readonly int? WebApiId;

        /// <summary>
        /// The characteristic.
        /// </summary>
        public readonly double Characteristic;

        /// <summary>
        /// The genes data.
        /// </summary>
        public readonly List<SubsequenceData> SubsequencesData;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceData"/> class.
        /// </summary>
        /// <param name="matterName">
        /// The matter name.
        /// </param>
        /// <param name="webApiId">
        /// Sequence web api id.
        /// </param>
        /// <param name="characteristic">
        /// The characteristic.
        /// </param>
        /// <param name="subsequencesData">
        /// The genes data.
        /// </param>
        public SequenceData(string matterName, int? webApiId, double characteristic, List<SubsequenceData> subsequencesData)
        {
            MatterName = matterName;
            WebApiId = webApiId;
            Characteristic = characteristic;
            SubsequencesData = subsequencesData;
        }
    }
}
