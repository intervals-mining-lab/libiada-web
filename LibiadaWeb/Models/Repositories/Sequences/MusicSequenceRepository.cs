namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaCore.Music;
    using LibiadaCore.Music.MusicXml;

    using LibiadaWeb.Helpers;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// The music sequence repository.
    /// </summary>
    public class MusicSequenceRepository : SequenceImporter, IMusicSequenceRepository
    {

        protected readonly FmotifRepository FmotifRepository;

        protected readonly MeasureRepository MeasureRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public MusicSequenceRepository(LibiadaWebEntities db) : base(db)
        {
            FmotifRepository = new FmotifRepository(db);
            MeasureRepository = new MeasureRepository(db);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="sequenceStream">
        /// The sequence stream.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if congeneric tracks count not equals 1 (track is not monophonic).
        /// </exception>
        public void Create(CommonSequence sequence, Stream sequenceStream)
        {
            string stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);
            var doc = new XmlDocument();
            doc.LoadXml(stringSequence);

            var parser = new MusicXmlParser();
            parser.Execute(doc);
            ScoreTrack tempTrack = parser.ScoreModel;

            if (tempTrack.CongenericScoreTracks.Count != 1)
            {
                throw new Exception("Track contains more then one or zero congeneric score tracks (parts).");
            }

            MatterRepository.CreateMatterFromSequence(sequence);

            BaseChain chain = ConvertCongenericScoreTrackToNotesBaseChain(tempTrack.CongenericScoreTracks[0]);
            long[] alphabet = ElementRepository.GetOrCreateNotesInDb(chain.Alphabet);
            Create(sequence, alphabet, chain.Building, Notation.Notes);

            chain = ConvertCongenericScoreTrackToMeasuresBaseChain(tempTrack.CongenericScoreTracks[0]);
            alphabet = MeasureRepository.GetOrCreateMeasuresInDb(chain.Alphabet);
            Create(sequence, alphabet, chain.Building, Notation.Measures);

            chain = ConvertCongenericScoreTrackToFormalMotifsBaseChain(tempTrack.CongenericScoreTracks[0]);
            alphabet = FmotifRepository.GetOrCreateFmotifsInDb(chain.Alphabet);
            Create(sequence, alphabet, chain.Building, Notation.FormalMotifs);
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
        public void Create(CommonSequence commonSequence, long[] alphabet, int[] building, Notation notation)
        {
            List<object> parameters = FillParams(commonSequence, alphabet, building, notation);

            const string Query = @"INSERT INTO music_chain (
                                        id,
                                        notation,
                                        matter_id,
                                        alphabet,
                                        building,
                                        remote_id,
                                        remote_db
                                    ) VALUES (
                                        @id,
                                        @notation,
                                        @matter_id,
                                        @alphabet,
                                        @building,
                                        @remote_id,
                                        @remote_db
                                    );";
            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
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
        private List<object> FillParams(CommonSequence commonSequence, long[] alphabet, int[] building, Notation notation)
        {
            commonSequence.Id = DbHelper.GetNewElementId(Db);

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
                    ParameterName = "notation",
                    NpgsqlDbType = NpgsqlDbType.Smallint,
                    Value = notation
                },
                new NpgsqlParameter
                {
                    ParameterName = "matter_id",
                    NpgsqlDbType = NpgsqlDbType.Bigint,
                    Value = commonSequence.MatterId
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
                    Value = (object)commonSequence.RemoteId ?? DBNull.Value
                },
                new NpgsqlParameter
                {
                    ParameterName = "remote_db",
                    NpgsqlDbType = NpgsqlDbType.Smallint,
                    Value = (object)commonSequence.RemoteDb ?? DBNull.Value
                }
            };
            return parameters;
        }
  
        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            Db.Dispose();
        }

        /// <summary>
        /// The convert congeneric score track to base chain.
        /// </summary>
        /// <param name="scoreTrack">
        /// The score track.
        /// </param>
        /// <returns>
        /// The <see cref="BaseChain"/>.
        /// </returns>
        private BaseChain ConvertCongenericScoreTrackToNotesBaseChain(CongenericScoreTrack scoreTrack)
        {
            List<ValueNote> notes = scoreTrack.GetNotes();
            return new BaseChain(((IEnumerable<IBaseObject>)notes).ToList());
        }

        /// <summary>
        /// Convert congeneric score track to measures base chain.
        /// </summary>
        /// <param name="scoreTrack">
        /// The score track.
        /// </param>
        /// <returns>
        /// The <see cref="BaseChain"/>.
        /// </returns>
        private BaseChain ConvertCongenericScoreTrackToMeasuresBaseChain(CongenericScoreTrack scoreTrack)
        {
            List<Measure> measures = scoreTrack.MeasureOrder();
            return new BaseChain(((IEnumerable<IBaseObject>)measures).ToList());
        }

        /// <summary>
        /// Converts congeneric score track to formal motifs base chain.
        /// </summary>
        /// <param name="scoreTrack">
        /// The score track.
        /// </param>
        /// <returns>
        /// The <see cref="BaseChain"/>.
        /// </returns>
        private BaseChain ConvertCongenericScoreTrackToFormalMotifsBaseChain(CongenericScoreTrack scoreTrack)
        {
            var borodaDivider = new BorodaDivider();
            FmotifChain fmotifChain = borodaDivider.Divide(scoreTrack, ParamPauseTreatment.Ignore, ParamEqualFM.NonSequent);
            return new BaseChain(((IEnumerable<IBaseObject>)fmotifChain.FmotifsList).ToList());
        }
    }
}
