namespace LibiadaWeb.Models.Repositories.Sequences
{
    using LibiadaWeb.Helpers;

    /// <summary>
    /// The data sequence repository.
    /// </summary>
    public class DataSequenceRepository : SequenceImporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public DataSequenceRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="commonSequence">
        /// The sequence.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Insert(CommonSequence commonSequence, long[] alphabet, int[] building)
        {
            var parameters = FillParams(commonSequence, alphabet, building);

            const string Query = @"INSERT INTO data_chain (
                                        id, 
                                        notation_id,
                                        matter_id, 
                                        piece_type_id, 
                                        piece_position, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id,
                                        @piece_type_id, 
                                        @piece_position,
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id
                                    );";

            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
        }
    }
}
