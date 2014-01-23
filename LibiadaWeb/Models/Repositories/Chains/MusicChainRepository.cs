using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Npgsql;
using NpgsqlTypes;

namespace LibiadaWeb.Models.Repositories.Chains
{ 
    public class MusicChainRepository : ChainImporter, IMusicChainRepository
    {
        private readonly LibiadaWebEntities db;

        public MusicChainRepository(LibiadaWebEntities db) : base(db)
        {
            this.db = db;
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

        public void Insert(chain chain, string fastaHeader, int? webApiId, long[] alphabet, int[] building)
        {
            var parameters = FillParams(chain, alphabet, building);
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "@fasta_header",
                NpgsqlDbType = NpgsqlDbType.Varchar,
                Value = fastaHeader
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "@web_api_id",
                NpgsqlDbType = NpgsqlDbType.Integer,
                Value = webApiId
            });

            String query = @"INSERT INTO dna_chain (
                                        id, 
                                        notation_id,
                                        matter_id, 
                                        dissimilar, 
                                        piece_type_id, 
                                        piece_position, 
                                        fasta_header, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id, 
                                        web_api_id
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id, 
                                        @dissimilar, 
                                        @piece_type_id, 
                                        @piece_position, 
                                        @fasta_header, 
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id, 
                                        @web_api_id
                                    );";
            db.ExecuteStoreCommand(query, parameters.ToArray());

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