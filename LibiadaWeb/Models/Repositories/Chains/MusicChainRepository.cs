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

        public IQueryable<music_chain> All
        {
            get { return db.music_chain; }
        }

        public IQueryable<music_chain> AllIncluding(params Expression<Func<music_chain, object>>[] includeProperties)
        {
            IQueryable<music_chain> query = db.music_chain;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public music_chain Find(long id)
        {
            return db.music_chain.Single(x => x.id == id);
        }

        public void Insert(chain chain, long[] alphabet, int[] building)
        {
            var parameters = FillParams(chain, alphabet, building);

            const string query = @"INSERT INTO music_chain (
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
            db.ExecuteStoreCommand(query, parameters.ToArray());

        }

        public void Insert(music_chain chain, long[] alphabet, int[] building)
        {
            Insert(ToChain(chain), alphabet, building);
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

        public void InsertOrUpdate(music_chain music_chain)
        {
            if (music_chain.id == default(long)) {
                // New entity
                throw new NotSupportedException("Для добавления новых записей следует использовать метод Insert.");
            } else {
                // Existing entity
                db.music_chain.Attach(music_chain);
                db.ObjectStateManager.ChangeObjectState(music_chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var music_chain = Find(id);
            db.music_chain.DeleteObject(music_chain);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose() 
        {
            db.Dispose();
        }
    }
}