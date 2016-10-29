namespace LibiadaWeb.Models.Repositories.Calculators
{
    /// <summary>
    /// The binary characteristic repository.
    /// </summary>
    public class BinaryCharacteristicRepository : IBinaryCharacteristicRepository
    {
        /// <summary>
        /// The create binary characteristic.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic id.
        /// </param>
        /// <param name="firstElementId">
        /// The first element id.
        /// </param>
        /// <param name="secondElementId">
        /// The second element id.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="BinaryCharacteristic"/>.
        /// </returns>
        public BinaryCharacteristic CreateBinaryCharacteristic(long sequenceId, int characteristicTypeLinkId, long firstElementId, long secondElementId, double value)
        {
            var characteristic = new BinaryCharacteristic
            {
                SequenceId = sequenceId,
                CharacteristicTypeLinkId = characteristicTypeLinkId,
                FirstElementId = firstElementId,
                SecondElementId = secondElementId,
                Value = value
            };
            return characteristic;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
