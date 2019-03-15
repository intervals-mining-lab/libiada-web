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

            BaseChain notesSequence = ConvertCongenericScoreTrackToNotesBaseChain(tempTrack.CongenericScoreTracks[0]);
            long[] notesAlphabet = ElementRepository.GetOrCreateNotesInDb(notesSequence.Alphabet);
            Create(sequence, notesAlphabet, notesSequence.Building, Notation.Notes);

            BaseChain measuresSequence = ConvertCongenericScoreTrackToMeasuresBaseChain(tempTrack.CongenericScoreTracks[0]);
            long[] measuresAlphabet = MeasureRepository.GetOrCreateMeasuresInDb(measuresSequence.Alphabet);
            Create(sequence, measuresAlphabet, measuresSequence.Building, Notation.Measures);

            foreach (PauseTreatment pauseTreatment in Enum.GetValues(typeof(PauseTreatment)))
            {
                if (pauseTreatment != PauseTreatment.NotApplicable)
                {
                    BaseChain fmotifsSecuence = ConvertCongenericScoreTrackToFormalMotifsBaseChain(tempTrack.CongenericScoreTracks[0], pauseTreatment, false);
                    long[] fmotifsAlphabet = FmotifRepository.GetOrCreateFmotifsInDb(fmotifsSecuence.Alphabet);
                    Create(sequence, fmotifsAlphabet, fmotifsSecuence.Building, Notation.FormalMotifs, pauseTreatment, false);

                    fmotifsSecuence = ConvertCongenericScoreTrackToFormalMotifsBaseChain(tempTrack.CongenericScoreTracks[0], pauseTreatment, true);
                    fmotifsAlphabet = FmotifRepository.GetOrCreateFmotifsInDb(fmotifsSecuence.Alphabet);
                    Create(sequence, fmotifsAlphabet, fmotifsSecuence.Building, Notation.FormalMotifs, pauseTreatment, true);
                }
            }
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
        public void Create(CommonSequence commonSequence, long[] alphabet, int[] building, Notation notation, PauseTreatment pauseTreatment = PauseTreatment.NotApplicable, bool sequentialTransfer = false)
        {
            List<object> parameters = FillParams(commonSequence, alphabet, building, notation, pauseTreatment, sequentialTransfer);

            const string Query = @"INSERT INTO music_chain (
                                        id,
                                        notation,
                                        matter_id,
                                        alphabet,
                                        building,
                                        remote_id,
                                        remote_db,
                                        pause_treatment,
                                        sequential_transfer
                                    ) VALUES (
                                        @id,
                                        @notation,
                                        @matter_id,
                                        @alphabet,
                                        @building,
                                        @remote_id,
                                        @remote_db,
                                        @pause_treatment,
                                        @sequential_transfer
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
        private List<object> FillParams(CommonSequence commonSequence, long[] alphabet, int[] building, Notation notation, PauseTreatment pauseTreatment, bool sequentialTransfer)
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
                },
                new NpgsqlParameter
                {
                    ParameterName = "pause_treatment",
                    NpgsqlDbType = NpgsqlDbType.Smallint,
                    Value = pauseTreatment
                },new NpgsqlParameter
                {
                    ParameterName = "sequential_transfer",
                    NpgsqlDbType = NpgsqlDbType.Boolean,
                    Value = sequentialTransfer
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
        private BaseChain ConvertCongenericScoreTrackToFormalMotifsBaseChain(CongenericScoreTrack scoreTrack, PauseTreatment pauseTreatment, bool sequentialTransfer)
        {
            var borodaDivider = new BorodaDivider();
            FmotifChain fmotifChain = borodaDivider.Divide(scoreTrack, pauseTreatment, sequentialTransfer);
            return new BaseChain(((IEnumerable<IBaseObject>)fmotifChain.FmotifsList).ToList());
        }
    }
}
