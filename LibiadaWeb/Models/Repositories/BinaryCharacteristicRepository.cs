namespace LibiadaWeb.Models.Repositories
{
    using System;

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
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
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
        /// The <see cref="long"/>.
        /// </returns>
        public long CreateBinaryCharacteristic(long sequenceId, int characteristicId, int? linkId, long firstElementId, long secondElementId, double value)
        {
            var characteristic = new BinaryCharacteristic
            {
                SequenceId = sequenceId, 
                CharacteristicTypeId = characteristicId, 
                LinkId = linkId, 
                FirstElementId = firstElementId, 
                SecondElementId = secondElementId, 
                Value = value, 
                ValueString = value.ToString()
            };
            db.BinaryCharacteristic.Add(characteristic);
            db.SaveChanges();
            return characteristic.Id;
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
