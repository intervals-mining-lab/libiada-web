namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
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

        /// <summary>
        /// The matters repository.
        /// </summary>
        protected readonly MatterRepository MatterRepository;

        /// <summary>
        /// The elements repository.
        /// </summary>
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
            MatterRepository = new MatterRepository(db);
            ElementRepository = new ElementRepository(db);
        }

        /// <summary>
        /// The fill parameters.
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
        protected List<NpgsqlParameter> FillParams(CommonSequence commonSequence, long[] alphabet, int[] building)
        {
            if (commonSequence.Id == default)
            {
                commonSequence.Id = Db.GetNewElementId();
            }

            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = commonSequence.Id },
                new NpgsqlParameter<byte>("notation", NpgsqlDbType.Smallint){ TypedValue = (byte)commonSequence.Notation },
                new NpgsqlParameter<long>("matter_id", NpgsqlDbType.Bigint){ TypedValue = commonSequence.MatterId },
                new NpgsqlParameter<long[]>("alphabet", NpgsqlDbType.Array | NpgsqlDbType.Bigint){ TypedValue = alphabet },
                new NpgsqlParameter<int[]>("building", NpgsqlDbType.Array | NpgsqlDbType.Integer){ TypedValue = building },
                new NpgsqlParameter<string>("remote_id", NpgsqlDbType.Varchar){ TypedValue = commonSequence.RemoteId  },
                new NpgsqlParameter("remote_db", NpgsqlDbType.Smallint){ Value = (object)((byte?)commonSequence.RemoteDb) ?? DBNull.Value },
            };
            return parameters;
        }
    }
}
