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

    /// <summary>
    /// The Fmotif repository.
    /// </summary>
    public class FmotifRepository : IFmotifRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="FmotifRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public FmotifRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The get or create Fmotifs in db.
        /// </summary>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <returns>
        /// The <see cref="T:long[]"/>.
        /// </returns>
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

        /// <summary>
        /// Saves Fmotifs to db.
        /// </summary>
        /// <param name="fmotif">
        /// The Fmotif.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        public long CreateFmotif(Fmotif fmotif)
        {
            var fmotifChain = new BaseChain(fmotif.NoteList.Cast<IBaseObject>().ToList());
            long[] notes = new ElementRepository(db).GetOrCreateNotesInDb(fmotifChain.Alphabet);

            var localFmotifHash = fmotif.GetHashCode().ToString();
            var dbFmotifs = db.Fmotif.Where(f => f.Value == localFmotifHash).ToList();
            if (dbFmotifs.Count > 0)
            {
                foreach (var dbFmotif in dbFmotifs)
                {
                    long[] dbAlphabet = db.GetFmotifAlphabet(dbFmotif.Id);
                    if (notes.SequenceEqual(dbAlphabet))
                    {
                        int[] dbBuilding = db.GetFmotifBuilding(dbFmotif.Id);
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

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="fmotif">
        /// The Fmotif.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        public long Create(Fmotif fmotif, long[] alphabet, int[] building)
        {
            List<NpgsqlParameter> parameters = FillParams(fmotif, alphabet, building);

            const string Query = @"INSERT INTO fmotif (
                                        id,
                                        value,
                                        notation,
                                        alphabet,
                                        building,
                                        fmotif_type
                                    ) VALUES (
                                        @id,
                                        @value,
                                        @notation,
                                        @alphabet,
                                        @building,
                                        @fmotif_type
                                    );";
            db.ExecuteCommand(Query, parameters.ToArray());
            return fmotif.Id;
        }

        /// <summary>
        /// The fill parameters.
        /// </summary>
        /// <param name="fmotif">
        /// The Fmotif.
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
        protected List<NpgsqlParameter> FillParams(Fmotif fmotif, long[] alphabet, int[] building)
        {
            fmotif.Id = db.GetNewElementId();
            var fmotivValue = fmotif.GetHashCode().ToString();
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = fmotif.Id },
                new NpgsqlParameter<string>("value", NpgsqlDbType.Varchar) { TypedValue = fmotivValue },
                new NpgsqlParameter<byte>("notation", NpgsqlDbType.Smallint) { TypedValue = (byte)Notation.FormalMotifs },
                new NpgsqlParameter<long[]>("alphabet", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { TypedValue = alphabet },
                new NpgsqlParameter<int[]>("building", NpgsqlDbType.Array | NpgsqlDbType.Integer) { TypedValue = building },
                new NpgsqlParameter<byte>("fmotif_type", NpgsqlDbType.Smallint) { TypedValue = (byte)fmotif.Type }
            };
            return parameters;
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
