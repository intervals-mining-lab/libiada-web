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
    /// The measure repository.
    /// </summary>
    public class MeasureRepository : IMeasureRepsitory
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public MeasureRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The get or create measure in db.
        /// </summary>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <returns></returns>
        public long[] GetOrCreateMeasuresInDb(Alphabet alphabet)
        {
            var result = new long[alphabet.Cardinality];
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                result[i] = CreateMeasure((Measure)alphabet[i]);
            }
            db.SaveChanges();
            return result;
        }

        /// <summary>
        /// Saves measures to db.
        /// </summary>
        /// <param name="measure">
        /// The measure.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        public long CreateMeasure(Measure measure)
        {
            var measureChain = new BaseChain(measure.NoteList.Cast<IBaseObject>().ToList());
            long[] notes = new ElementRepository(db).GetOrCreateNotesInDb(measureChain.Alphabet);

            string localMeasureHash = measure.GetHashCode().ToString();
            var dbMeasures = db.Measure.Where(m => m.Value == localMeasureHash).ToList();
            if (dbMeasures.Count > 0)
            {
                foreach (var dbMeasure in dbMeasures)
                {
                    long[] dbAlphabet = db.GetMeasureAlphabet(dbMeasure.Id);
                    if (notes.SequenceEqual(dbAlphabet))
                    {
                        int[] dbBuilding = db.GetMeasureBuilding(dbMeasure.Id);
                        if (measureChain.Building.SequenceEqual(dbBuilding))
                        {
                            if (measure.Attributes.Key.Fifths != dbMeasure.Fifths
                                || measure.Attributes.Size.BeatBase != dbMeasure.Beatbase
                                || measure.Attributes.Size.Beats != dbMeasure.Beats)
                            {
                                throw new Exception("Found in db measure is not equal to local measure.");
                            }

                            return dbMeasure.Id;
                        }
                    }
                }
            }

            return Create(measure, notes, measureChain.Building);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="measure">
        /// The measure.
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
        public long Create(Measure measure, long[] alphabet, int[] building)
        {
            List<NpgsqlParameter> parameters = FillParams(measure, alphabet, building);

            const string Query = @"INSERT INTO measure (
                                        id,
                                        value,
                                        notation,
                                        alphabet,
                                        building,
                                        beats,
                                        beatbase,
                                        fifths,
                                        major
                                    ) VALUES (
                                        @id,
                                        @value,
                                        @notation,
                                        @alphabet,
                                        @building,
                                        @beats,
                                        @beatbase,
                                        @fifths,
                                        @major
                                    );";
            db.ExecuteCommand(Query, parameters.ToArray());
            return measure.Id;
        }

        /// <summary>
        /// The fill parameters.
        /// </summary>
        /// <param name="measure">
        /// The measure.
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
        protected List<NpgsqlParameter> FillParams(Measure measure, long[] alphabet, int[] building)
        {
            measure.Id = db.GetNewElementId();
            var measureValue = measure.GetHashCode().ToString();
            var mode = measure.Attributes.Key.Mode;

            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue =  measure.Id },
                new NpgsqlParameter<string>("value", NpgsqlDbType.Varchar) { TypedValue =  measureValue },
                new NpgsqlParameter<byte>("notation", NpgsqlDbType.Smallint) { TypedValue =  (byte)Notation.Measures },
                new NpgsqlParameter<long[]>("alphabet", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { TypedValue =  alphabet },
                new NpgsqlParameter<int[]>("building", NpgsqlDbType.Array | NpgsqlDbType.Integer) { TypedValue =  building },
                new NpgsqlParameter<int>("beats", NpgsqlDbType.Integer) { TypedValue =  measure.Attributes.Size.Beats },
                new NpgsqlParameter<int>("beatbase", NpgsqlDbType.Integer) { TypedValue =  measure.Attributes.Size.BeatBase },
                new NpgsqlParameter<int>("fifths", NpgsqlDbType.Integer) { TypedValue =  measure.Attributes.Key.Fifths },
                new NpgsqlParameter<bool>("major", NpgsqlDbType.Boolean) { TypedValue =  (mode.Equals("major") || mode.Equals(null)) }
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