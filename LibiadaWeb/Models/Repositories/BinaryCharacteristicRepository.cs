namespace LibiadaWeb.Models.Repositories
{
    /// <summary>
    /// The binary characteristic repository.
    /// </summary>
    public class BinaryCharacteristicRepository : IBinaryCharacteristicRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public BinaryCharacteristicRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

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
        public void CreateBinaryCharacteristic(long sequenceId, int characteristicTypeLinkId, long firstElementId, long secondElementId, double value)
        {
            var characteristic = new BinaryCharacteristic
            {
                SequenceId = sequenceId,
                CharacteristicTypeLinkId = characteristicTypeLinkId, 
                FirstElementId = firstElementId, 
                SecondElementId = secondElementId, 
                Value = value
            };
            db.BinaryCharacteristic.Add(characteristic);
            db.SaveChanges();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose() 
        {
            db.Dispose();
        }
    }
}
