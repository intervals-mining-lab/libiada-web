namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;

    using LibiadaWeb.Helpers;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The sequence importer.
    /// </summary>
    public abstract class SequenceImporter
    {
        /// <summary>
        /// The db.
        /// </summary>
        protected readonly LibiadaWebEntities Db;

        protected readonly MatterRepository MatterRepository;

        protected readonly ElementRepository ElementRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceImporter"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        protected SequenceImporter(LibiadaWebEntities db)
        {
            Db = db;
            this.MatterRepository = new MatterRepository(db);
            this.ElementRepository = new ElementRepository(db);
        }

        /// <summary>
        /// The fill params.
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
        /// <returns>
        /// The <see cref="List{Object}"/>.
        /// </returns>
        protected List<object> FillParams(CommonSequence commonSequence, long[] alphabet, int[] building)
        {
            if (commonSequence.Id == default(long))
            {
                commonSequence.Id = DbHelper.GetNewElementId(Db);
            }

            var parameters = new List<object>
            {
                new NpgsqlParameter
                {
                    ParameterName = "id", 
                    NpgsqlDbType = NpgsqlDbType.Bigint, 
                    Value = commonSequence.Id
                }, 
                new NpgsqlParameter
                {
                    ParameterName = "notation_id", 
                    NpgsqlDbType = NpgsqlDbType.Integer, 
                    Value = commonSequence.NotationId
                }, 
                new NpgsqlParameter
                {
                    ParameterName = "matter_id", 
                    NpgsqlDbType = NpgsqlDbType.Bigint, 
                    Value = commonSequence.MatterId
                }, 
                new NpgsqlParameter
                {
                    ParameterName = "piece_type_id", 
                    NpgsqlDbType = NpgsqlDbType.Integer, 
                    Value = commonSequence.PieceTypeId
                }, 
                new NpgsqlParameter
                {
                    ParameterName = "alphabet", 
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Bigint, 
                    Value = alphabet
                }, 
                new NpgsqlParameter
                {
                    ParameterName = "building", 
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer, 
                    Value = building
                }, 
                new NpgsqlParameter
                {
                    ParameterName = "remote_id", 
                    NpgsqlDbType = NpgsqlDbType.Varchar, 
                    Value = commonSequence.RemoteId
                }, 
                new NpgsqlParameter
                {
                    ParameterName = "remote_db_id", 
                    NpgsqlDbType = NpgsqlDbType.Integer, 
                    Value = commonSequence.RemoteDbId
                }, 
                new NpgsqlParameter
                {
                    ParameterName = "piece_position", 
                    NpgsqlDbType = NpgsqlDbType.Integer, 
                    Value = commonSequence.PiecePosition
                }
            };
            return parameters;
        }
    }
}
