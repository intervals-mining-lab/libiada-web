using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Chains
{ 
    public class MusicChainRepository : ChainImporter, IMusicChainRepository
    {
        public MusicChainRepository(LibiadaWebEntities db) : base(db)
        {
        }

        public void Insert(chain chain, long[] alphabet, int[] building)
        {
            var parameters = FillParams(chain, alphabet, building);

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

        public void Dispose() 
        {
            db.Dispose();
        }
    }
}