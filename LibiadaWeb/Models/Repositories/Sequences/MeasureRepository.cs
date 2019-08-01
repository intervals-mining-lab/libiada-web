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
            var measureChain= new BaseChain(measure.NoteList.Cast<IBaseObject>().ToList());
            long[] notes = new ElementRepository(db).GetOrCreateNotesInDb(measureChain.Alphabet);

            string localMeasureHash = BitConverter.ToString(measure.GetMD5HashCode()).Replace("-", string.Empty);
            var dbMeasures = db.Measure.Where(m => m.Value == localMeasureHash).ToList();
            if (dbMeasures.Count > 0)
            {
                foreach (var dbMeasure in dbMeasures)
                {
                    var dbAlphabet = DbHelper.GetMeasureAlphabet(db, dbMeasure.Id);
                    if (notes.SequenceEqual(dbAlphabet))
                    {
                        var dbBuilding = DbHelper.GetMeasureBuilding(db, dbMeasure.Id);
                        if (measureChain.Building.SequenceEqual(dbBuilding))
                        {
                            if (measure.Attributes.Key.Fifths != dbMeasure.Fifths
                                || measure.Attributes.Size.BeatBase != dbMeasure.Beatbase
                                || measure.Attributes.Size.BeatBase != dbMeasure.Beats
                                || measure.Attributes.Size.TicksPerBeat != dbMeasure.TicksPerBeat)
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
            List<object> parameters = FillParams(measure, alphabet, building);

            const string Query = @"INSERT INTO measure (
                                        id,
                                        value,
                                        notation,
                                        alphabet,
                                        building,
                                        beats,
                                        beatbase,
                                        ticks_per_beat,
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
                                        @ticks_per_beat,
                                        @fifths,
                                        @major
                                    );";
            DbHelper.ExecuteCommand(db, Query, parameters.ToArray());
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
        protected List<object> FillParams(Measure measure, long[] alphabet, int[] building)
        {
            measure.Id = DbHelper.GetNewElementId(db);

            var parameters = new List<object>
            {
                new NpgsqlParameter
                {
                    ParameterName = "id",
                    NpgsqlDbType = NpgsqlDbType.Bigint,
                    Value = measure.Id
                },
                new NpgsqlParameter
                {
                    ParameterName = "value",
                    NpgsqlDbType = NpgsqlDbType.Varchar,
                    Value = BitConverter.ToString(measure.GetMD5HashCode()).Replace("-", string.Empty)
                },
                new NpgsqlParameter
                {
                    ParameterName = "notation",
                    NpgsqlDbType = NpgsqlDbType.Smallint,
                    Value = Notation.Measures
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
                    ParameterName = "beats",
                    NpgsqlDbType = NpgsqlDbType.Integer,
                    Value = measure.Attributes.Size.Beats
                },
                new NpgsqlParameter
                {
                    ParameterName = "beatbase",
                    NpgsqlDbType = NpgsqlDbType.Integer,
                    Value = measure.Attributes.Size.BeatBase
                },
                new NpgsqlParameter
                {
                    ParameterName = "ticks_per_beat",
                    NpgsqlDbType = NpgsqlDbType.Integer,
                    Value = measure.Attributes.Size.TicksPerBeat
                },
                new NpgsqlParameter
                {
                    ParameterName = "fifths",
                    NpgsqlDbType = NpgsqlDbType.Integer,
                    Value = measure.Attributes.Key.Fifths
                },
                new NpgsqlParameter
                {
                    ParameterName = "major",
                    NpgsqlDbType = NpgsqlDbType.Boolean,
                    Value = (measure.Attributes.Key.Mode.Equals("major") || measure.Attributes.Key.Mode.Equals(null)) ? true : false
                }
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