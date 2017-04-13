namespace LibiadaWeb.Models.Repositories.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The characteristic repository.
    /// </summary>
    public class CharacteristicRepository : ICharacteristicRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CharacteristicRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The try save characteristics to database.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        public void TrySaveCharacteristicsToDatabase(List<CharacteristicValue> characteristics)
        {
            if (characteristics.Count > 0)
            {
                try
                {
                    db.CharacteristicValue.AddRange(characteristics);
                    db.SaveChanges();
                }
                catch (Exception exception)
                {
                    // todo: refactor and optimize all this
                    var characteristicsSequences = characteristics.Select(c => c.SequenceId).Distinct().ToArray();
                    var characteristicsTypes = characteristics.Select(c => c.CharacteristicLinkId).Distinct().ToArray();
                    var characteristicsFilter = characteristics.Select(c => new { c.SequenceId, c.CharacteristicLinkId }).ToArray();
                    var wasteCharacteristics = db.CharacteristicValue.Where(c => characteristicsSequences.Contains(c.SequenceId) && characteristicsTypes.Contains(c.CharacteristicLinkId))
                            .ToArray().Where(c => characteristicsFilter.Contains(new { c.SequenceId, c.CharacteristicLinkId })).Select(c => new { c.SequenceId, c.CharacteristicLinkId });
                    var wasteNewCharacteristics = characteristics.Where(c => wasteCharacteristics.Contains(new { c.SequenceId, c.CharacteristicLinkId }));

                    db.CharacteristicValue.RemoveRange(wasteNewCharacteristics);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception anotherException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
