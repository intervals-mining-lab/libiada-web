namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    using LibiadaCore.Core;

    using LibiadaMusic.MusicXml;
    using LibiadaMusic.ScoreModel;

    using LibiadaWeb.Helpers;

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
        /// <param name="stringSequence">
        /// The string sequence.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if congeneric tracks count not equals 1 (track is not monophonic).
        /// </exception>
        public void Create(CommonSequence sequence, string stringSequence)
        {
            var doc = new XmlDocument();
            doc.LoadXml(stringSequence);

            MusicXmlParser parser = new MusicXmlParser();
            parser.Execute(doc, "test");
            ScoreTrack tempTrack = parser.ScoreModel;

            if (tempTrack.CongenericScoreTracks.Count != 1)
            {
                throw new Exception("Track contains more then one or zero congeneric score tracks (parts).");
            }
            
            var chain = ConvertCongenericScoreTrackToBaseChain(tempTrack.CongenericScoreTracks[0]);

            MatterRepository.CreateMatterFromSequence(sequence);

            var alphabet = ElementRepository.GetOrCreateNotesInDb(chain.Alphabet);
            Create(sequence, alphabet, chain.Building);
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
            var parameters = FillParams(commonSequence, alphabet, building);

            const string Query = @"INSERT INTO music_chain (
                                        id, 
                                        notation_id,
                                        matter_id,
                                        feature_id, 
                                        piece_position,  
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id,
                                        @feature_id, 
                                        @piece_position,  
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id
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
        private BaseChain ConvertCongenericScoreTrackToBaseChain(CongenericScoreTrack scoreTrack)
        {
            var notes = scoreTrack.GetNotes();
            return new BaseChain(((IEnumerable<IBaseObject>)notes).ToList());
        }
    }
}
