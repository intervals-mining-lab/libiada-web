namespace LibiadaWeb.Models.Repositories.Chains
{
    /// <summary>
    /// The music chain repository.
    /// </summary>
    public class MusicChainRepository : ChainImporter, IMusicChainRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MusicChainRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public MusicChainRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="chain">
        /// The chain.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Insert(chain chain, long[] alphabet, int[] building)
        {
            var parameters = this.FillParams(chain, alphabet, building);

            const string Query = @"INSERT INTO music_chain (
                                        id, 
                                        notation_id,
                                        matter_id, 
                                        dissimilar, 
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
                                        @dissimilar, 
                                        @piece_type_id, 
                                        @piece_position,  
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id
                                    );";
            db.Database.ExecuteSqlCommand(Query, parameters.ToArray());
        }

        /// <summary>
        /// The to chain.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="chain"/>.
        /// </returns>
        public chain ToChain(music_chain source)
        {
            return new chain
            {
                id = source.id, 
                dissimilar = source.dissimilar, 
                notation_id = source.notation_id, 
                matter_id = source.matter_id, 
                piece_type_id = source.piece_type_id, 
                piece_position = source.piece_position
            };
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