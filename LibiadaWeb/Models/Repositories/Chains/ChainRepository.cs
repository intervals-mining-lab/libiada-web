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
    public class CommonSequenceRepository : ChainImporter, ICommonSequenceRepository
    {
        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CommonSequenceRepository(LibiadaWebEntities db) : base(db)
        {
            elementRepository = new ElementRepository(db);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="commonSequence">
        /// The chain.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Insert(CommonSequence commonSequence, long[] alphabet, int[] building)
        {
            var parameters = FillParams(commonSequence, alphabet, building);

            const string Query = @"INSERT INTO chain (
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
            db.Database.ExecuteSqlCommand(Query, parameters.ToArray());
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="selectedSequences">
        /// The selected chains.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<CommonSequence> selectedSequences)
        {
            return GetSelectListItems(null, selectedSequences);
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="allSequences">
        /// The all chains.
        /// </param>
        /// <param name="selectedSequences">
        /// The selected chains.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<CommonSequence> allSequences, IEnumerable<CommonSequence> selectedSequences)
        {
            if (allSequences == null)
            {
                allSequences = db.CommonSequence.Include("matter");
            }

            HashSet<long> chainIds = selectedSequences != null
                                          ? new HashSet<long>(selectedSequences.Select(c => c.Id))
                                          : new HashSet<long>();
            var chainsList = new List<SelectListItem>();
            
            foreach (var sequence in allSequences)
            {
                chainsList.Add(new SelectListItem
                {
                    Value = sequence.Id.ToString(), 
                    Text = sequence.Matter.Name, 
                    Selected = chainIds.Contains(sequence.Id)
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
        /// The <see cref="List{element}"/>.
        /// </returns>
        public List<Element> GetElements(long chainId)
        {
            List<long> elementIds = GetElementIds(chainId);
            return elementRepository.GetElements(elementIds);
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
            List<long> elements = GetElementIds(chainId);
            return elementRepository.ToLibiadaAlphabet(elements);
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
            return db.Database.SqlQuery<long>(Query, new NpgsqlParameter("@id", chainId)).ToList();
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
            return db.Database.SqlQuery<int>(Query, new NpgsqlParameter("@id", chainId)).ToArray();
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
        public BaseChain ToLibiadaBaseChain(long chainId)
        {
            return new BaseChain(GetBuilding(chainId), GetAlphabet(chainId));
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
            return new Chain(GetBuilding(chainId), GetAlphabet(chainId));
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
