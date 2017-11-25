namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using LibiadaCore.Core;

    using LibiadaMusic.MusicXml;
    using LibiadaMusic.ScoreModel;

    using LibiadaWeb.Helpers;
    using LibiadaMusic.BorodaDivider;

    /// <summary>
    /// The music sequence repository.
    /// </summary>
    public class MusicSequenceRepository : SequenceImporter, IMusicSequenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MusicSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public MusicSequenceRepository(LibiadaWebEntities db) : base(db)
        {
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
            parser.Execute(doc, "test");
            ScoreTrack tempTrack = parser.ScoreModel;

            if (tempTrack.CongenericScoreTracks.Count != 1)
            {
                throw new Exception("Track contains more then one or zero congeneric score tracks (parts).");
            }

            switch (sequence.Notation)
            {
                case Notation.Notes:
                    {
                        BaseChain chain = ConvertCongenericScoreTrackToNotesBaseChain(tempTrack.CongenericScoreTracks[0]);

                        MatterRepository.CreateMatterFromSequence(sequence);

                        long[] alphabet = ElementRepository.GetOrCreateNotesInDb(chain.Alphabet);
                        Create(sequence, alphabet, chain.Building);

                        break;
                    }
                case Notation.Measures:
                    {
                        BaseChain chain = ConvertCongenericScoreTrackToMeasuresBaseChain(tempTrack.CongenericScoreTracks[0]);
                        break;
                    }
                case Notation.FormalMotifs:
                    {
                        BaseChain chain = ConvertCongenericScoreTrackToFormalMotifsBaseChain(tempTrack.CongenericScoreTracks[0]);
                        break;
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
        public void Create(CommonSequence commonSequence, long[] alphabet, int[] building)
        {
            List<object> parameters = FillParams(commonSequence, alphabet, building);

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

        private BaseChain ConvertCongenericScoreTrackToMeasuresBaseChain(CongenericScoreTrack scoreTrack)
        {
            List<Measure> measures = scoreTrack.MeasureOrder();
            return new BaseChain(((IEnumerable<IBaseObject>)measures).ToList());
        }

        private BaseChain ConvertCongenericScoreTrackToFormalMotifsBaseChain(CongenericScoreTrack scoreTrack)
        {
            BorodaDivider borodaDivider = new BorodaDivider();
            FmotivChain fMotifChain = borodaDivider.Divide(scoreTrack, ParamPauseTreatment.Ignore, ParamEqualFM.NonSequent);
            return new BaseChain(((IEnumerable<IBaseObject>)fMotifChain).ToList());
        }
    }
}
