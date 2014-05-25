// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DnaChainRepository.cs" company="">
//   
// </copyright>
// <summary>
//   The dna chain repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibiadaWeb.Models.Repositories.Chains
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The dna chain repository.
    /// </summary>
    public class DnaChainRepository : ChainImporter, IDnaChainRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnaChainRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public DnaChainRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="chain">
        /// The chain.
        /// </param>
        /// <param name="fastaHeader">
        /// The fasta header.
        /// </param>
        /// <param name="webApiId">
        /// The web api id.
        /// </param>
        /// <param name="productId">
        /// The product id.
        /// </param>
        /// <param name="complement">
        /// The complement.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Insert(
            chain chain, 
            string fastaHeader, 
            int? webApiId, 
            int? productId, 
            bool complement, 
            bool partial, 
            long[] alphabet, 
            int[] building)
        {
            var parameters = this.FillParams(chain, alphabet, building);
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "fasta_header", 
                NpgsqlDbType = NpgsqlDbType.Varchar, 
                Value = fastaHeader
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "web_api_id", 
                NpgsqlDbType = NpgsqlDbType.Integer, 
                Value = webApiId
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "product_id", 
                NpgsqlDbType = NpgsqlDbType.Integer, 
                Value = productId
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "partial", 
                NpgsqlDbType = NpgsqlDbType.Boolean, 
                Value = partial
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "complement", 
                NpgsqlDbType = NpgsqlDbType.Boolean, 
                Value = complement
            });

            const string Query = @"INSERT INTO dna_chain (
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
                                        web_api_id,
                                        product_id,
                                        partial,
                                        complement
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
                                        @web_api_id,
                                        @product_id,
                                        @partial,
                                        @complement
                                    );";
            this.db.Database.ExecuteSqlCommand(Query, parameters.ToArray());
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
        public void Insert(dna_chain chain, long[] alphabet, int[] building)
        {
            this.Insert(this.ToChain(chain), chain.fasta_header, chain.web_api_id, null, false, false, alphabet, building);
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
        public chain ToChain(dna_chain source)
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
        /// The get select list items.
        /// </summary>
        /// <param name="chains">
        /// The chains.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<dna_chain> chains)
        {
            return this.GetSelectListItems(this.db.dna_chain.ToList(), chains);
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="allChains">
        /// The all chains.
        /// </param>
        /// <param name="selectedChain">
        /// The selected chain.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<dna_chain> allChains, 
            IEnumerable<dna_chain> selectedChain)
        {
            HashSet<long> chainIds = selectedChain != null
                ? new HashSet<long>(selectedChain.Select(c => c.id))
                : new HashSet<long>();
            if (allChains == null)
            {
                allChains = this.db.dna_chain.Include("matter");
            }

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
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.db.Dispose();
        }
    }
}