namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Xml;

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
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        /// <param name="stringSequence">
        /// The string sequence.
        /// </param>
        public void Create(CommonSequence commonSequence, string stringSequence)
        {
            var doc = new XmlDocument();
            doc.LoadXml(stringSequence);

            // MusicXmlParser parser = new MusicXmlParser();
            // parser.Execute(doc, "test");
            // ScoreTrack tempTrack = parser.ScoreModel;
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
                                        piece_type_id, 
                                        piece_position,  
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id,
                                        @piece_type_id, 
                                        @piece_position,  
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id
                                    );";
            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
        }

        /// <summary>
        /// The to sequence.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="CommonSequence"/>.
        /// </returns>
        public CommonSequence ToCommonSequence(MusicSequence source)
        {
            return new CommonSequence
            {
                Id = source.Id,
                NotationId = source.NotationId, 
                MatterId = source.MatterId, 
                PieceTypeId = source.PieceTypeId, 
                PiecePosition = source.PiecePosition
            };
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose() 
        {
            Db.Dispose();
        }
    }
}
