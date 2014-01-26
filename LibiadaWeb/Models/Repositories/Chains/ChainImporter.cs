using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NpgsqlTypes;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public abstract class ChainImporter
    {
        protected readonly LibiadaWebEntities db;

        protected ChainImporter(LibiadaWebEntities db)
        {
            this.db = db;
        }

        protected List<object> FillParams(chain chain, long[] alphabet, int[] building)
        {
            if (chain.id == default(long))
            {
                chain.id = db.ExecuteStoreQuery<long>("SELECT nextval('elements_id_seq');").First();
            }

            var parameters = new List<object>
            {
                new NpgsqlParameter
                {
                    ParameterName = "id",
                    NpgsqlDbType = NpgsqlDbType.Bigint,
                    Value = chain.id
                },
                new NpgsqlParameter
                {
                    ParameterName = "notation_id",
                    NpgsqlDbType = NpgsqlDbType.Integer,
                    Value = chain.notation_id
                },
                new NpgsqlParameter
                {
                    ParameterName = "matter_id",
                    NpgsqlDbType = NpgsqlDbType.Bigint,
                    Value = chain.matter_id
                },
                new NpgsqlParameter
                {
                    ParameterName = "piece_type_id",
                    NpgsqlDbType = NpgsqlDbType.Integer,
                    Value = chain.piece_type_id
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
                    Value = chain.remote_id
                },
                new NpgsqlParameter
                {
                    ParameterName = "remote_db_id",
                    NpgsqlDbType = NpgsqlDbType.Integer,
                    Value = chain.remote_db_id
                },
                new NpgsqlParameter
                {
                    ParameterName = "piece_position",
                    NpgsqlDbType = NpgsqlDbType.Integer,
                    Value = chain.piece_position
                },
                new NpgsqlParameter
                {
                    ParameterName = "dissimilar",
                    NpgsqlDbType = NpgsqlDbType.Boolean,
                    Value = chain.dissimilar
                }
            };
            return parameters;
        }
    }
}