namespace LibiadaWeb.Models.Repositories.Chains
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using Npgsql;

    /// <summary>
    /// The chain repository.
    /// </summary>
    public class ChainRepository : ChainImporter, IChainRepository
    {
        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public ChainRepository(LibiadaWebEntities db) : base(db)
        {
            this.elementRepository = new ElementRepository(db);
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

            const string Query = @"INSERT INTO chain (
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
            this.db.Database.ExecuteSqlCommand(Query, parameters.ToArray());
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="selectedChains">
        /// The selected chains.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> selectedChains)
        {
            return this.GetSelectListItems(null, selectedChains);
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="allChains">
        /// The all chains.
        /// </param>
        /// <param name="selectedChains">
        /// The selected chains.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> allChains, IEnumerable<chain> selectedChains)
        {
            if (allChains == null)
            {
                allChains = this.db.chain.Include("matter");
            }

            HashSet<long> chainIds = selectedChains != null
                                          ? new HashSet<long>(selectedChains.Select(c => c.id))
                                          : new HashSet<long>();
            var chainsList = new List<SelectListItem>();
            
            foreach (var chain in allChains)
            {
                chainsList.Add(new SelectListItem
                {
                    Value = chain.id.ToString(), 
                    Text = chain.matter.name, 
                    Selected = chainIds.Contains(chain.id)
                });
            }

            return chainsList;
        }

        /// <summary>
        /// The get elements.
        /// </summary>
        /// <param name="chainId">
        /// The chain id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<element> GetElements(long chainId)
        {
            
            List<long> elementIds = this.GetElementIds(chainId);
            return this.elementRepository.GetElements(elementIds);
        }

        /// <summary>
        /// The get alphabet.
        /// </summary>
        /// <param name="chainId">
        /// The chain id.
        /// </param>
        /// <returns>
        /// The <see cref="Alphabet"/>.
        /// </returns>
        public Alphabet GetAlphabet(long chainId)
        {
            List<long> elements = this.GetElementIds(chainId);
            return this.elementRepository.ToLibiadaAlphabet(elements);
        }

        /// <summary>
        /// The get element ids.
        /// </summary>
        /// <param name="chainId">
        /// The chain id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<long> GetElementIds(long chainId)
        {
            const string Query = "SELECT unnest(alphabet) FROM chain WHERE id = @id";
            return this.db.Database.SqlQuery<long>(Query, new NpgsqlParameter("@id", chainId)).ToList();
        }

        /// <summary>
        /// The get building.
        /// </summary>
        /// <param name="chainId">
        /// The chain id.
        /// </param>
        /// <returns>
        /// The <see cref="int[]"/>.
        /// </returns>
        public int[] GetBuilding(long chainId)
        {
            const string Query = "SELECT unnest(building) FROM chain WHERE id = @id";
            return this.db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", chainId)).ToArray();
        }

        /// <summary>
        /// The to l base chain.
        /// </summary>
        /// <param name="chainId">
        /// The chain id.
        /// </param>
        /// <returns>
        /// The <see cref="BaseChain"/>.
        /// </returns>
        public BaseChain ToLBaseChain(long chainId)
        {
            return new BaseChain(this.GetBuilding(chainId), this.GetAlphabet(chainId));
        }

        /// <summary>
        /// The to libiada chain.
        /// </summary>
        /// <param name="chainId">
        /// The chain id.
        /// </param>
        /// <returns>
        /// The <see cref="Chain"/>.
        /// </returns>
        public Chain ToLibiadaChain(long chainId)
        {
            return new Chain(this.GetBuilding(chainId), this.GetAlphabet(chainId));
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.db.Dispose();
        }
    }
}