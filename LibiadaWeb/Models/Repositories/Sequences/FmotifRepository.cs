namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaWeb.Helpers;
    using Npgsql;
    using NpgsqlTypes;

    public class FmotifRepository : IFmotifRepository
    {
        private readonly LibiadaWebEntities db;

        public FmotifRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public long[] GetOrCreateFmotifsInDb(Alphabet alphabet)
        {
            var result = new long[alphabet.Cardinality];
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                result[i] = CreateFmotif((Fmotif)alphabet[i]);
            }
            db.SaveChanges();
            return result;
        }

        public long CreateFmotif(Fmotif fmotif)
        {
            var fmotifChain = new BaseChain(fmotif.NoteList.Cast<IBaseObject>().ToList());
            long[] notes = new ElementRepository(db).GetOrCreateNotesInDb(fmotifChain.Alphabet);

            string localFmotifHash = BitConverter.ToString(fmotif.GetMD5HashCode()).Replace("-", string.Empty);
            var dbFmotifs = db.Fmotif.Where(f => f.Value == localFmotifHash).ToList();
            if (dbFmotifs.Count > 0)
            {
                foreach (var dbFmotif in dbFmotifs)
                {
                    var dbAlphabet = DbHelper.GetFmotifAlphabet(db, dbFmotif.Id);
                    if (notes.SequenceEqual(dbAlphabet))
                    {
                        var dbBuilding = DbHelper.GetFmotifBuilding(db, dbFmotif.Id);
                        if (fmotifChain.Building.SequenceEqual(dbBuilding))
                        {
                            if (fmotif.Type != dbFmotif.FmotifType)
                            {
                                throw new Exception("Found in db fmotif is not equal to local fmotif.");
                            }

                            return dbFmotif.Id;
                        }
                    }
                }
            }
            
            return Create(fmotif, notes, fmotifChain.Building);
        }

        public long Create(Fmotif fmotif, long[] alphabet, int[] building)
        {
            List<object> parameters = FillParams(fmotif, alphabet, building);

            const string Query = @"INSERT INTO fmotif (
                                        id,
                                        value,
                                        description,
                                        name,
                                        notation,
                                        alphabet,
                                        building,
                                        fmotif_type
                                    ) VALUES (
                                        @id,
                                        @value,
                                        @description,
                                        @name,
                                        @notation,
                                        @alphabet,
                                        @building,
                                        @fmotif_type
                                    );";
            DbHelper.ExecuteCommand(db, Query, parameters.ToArray());
            return fmotif.Id;
        }

        protected List<object> FillParams(Fmotif fmotif, long[] alphabet, int[] building)
        {
            fmotif.Id = DbHelper.GetNewElementId(db);

            var parameters = new List<object>
            {
                new NpgsqlParameter
                {
                    ParameterName = "id",
                    NpgsqlDbType = NpgsqlDbType.Bigint,
                    Value = fmotif.Id
                },
                new NpgsqlParameter
                {
                    ParameterName = "value",
                    NpgsqlDbType = NpgsqlDbType.Varchar,
                    Value = BitConverter.ToString(fmotif.GetMD5HashCode()).Replace("-", string.Empty)
                },
                new NpgsqlParameter
                {
                    ParameterName = "description",
                    NpgsqlDbType = NpgsqlDbType.Text,
                    Value = ""
                },
                new NpgsqlParameter
                {
                    ParameterName = "name",
                    NpgsqlDbType = NpgsqlDbType.Varchar,
                    Value = ""
                },
                new NpgsqlParameter
                {
                    ParameterName = "notation",
                    NpgsqlDbType = NpgsqlDbType.Smallint,
                    Value = Notation.FormalMotifs
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
                    ParameterName = "fmotif_type",
                    NpgsqlDbType = NpgsqlDbType.Smallint,
                    Value = fmotif.Type
                }
            };
            return parameters;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
