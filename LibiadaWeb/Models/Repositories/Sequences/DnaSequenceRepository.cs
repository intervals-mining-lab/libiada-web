namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The dna sequence repository.
    /// </summary>
    public class DnaSequenceRepository : SequenceImporter, IDnaSequenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnaSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public DnaSequenceRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// The create dna sequence.
        /// </summary>
        /// <param name="sequence">
        /// The common sequence.
        /// </param>
        /// <param name="productId">
        /// The product id.
        /// </param>
        /// <param name="partial">
        /// The partial.
        /// </param>
        /// <param name="complementary">
        /// The complementary.
        /// </param>
        /// <param name="stringSequence">
        /// The string sequence.
        /// </param>
        /// <param name="webApiId">
        /// The web api id.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if at least one element of new sequence is missing in db.
        /// </exception>
        public void Create(CommonSequence sequence, bool partial, bool complementary, string stringSequence, int? webApiId)
        {
            // отделяем заголовок fasta файла от цепочки
            string[] splittedFasta = stringSequence.Split('\n', '\r');
            var sequenceStringBuilder = new StringBuilder();
            string fastaHeader = splittedFasta[0];
            for (int j = 1; j < splittedFasta.Length; j++)
            {
                sequenceStringBuilder.Append(splittedFasta[j]);
            }

            string resultStringSequence = DataTransformers.CleanFastaFile(sequenceStringBuilder.ToString());

            var chain = new BaseChain(resultStringSequence);

            if (!ElementRepository.ElementsInDb(chain.Alphabet, sequence.NotationId))
            {
                throw new Exception("At least one element of new sequence is missing in db.");
            }

            MatterRepository.CreateMatterFromSequence(sequence);

            var alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.NotationId, false);
            Create(sequence, fastaHeader, webApiId, complementary, partial, alphabet, chain.Building);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="sequence">
        /// The sequence.
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
        /// <param name="complementary">
        /// The complementary.
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
        public void Create(CommonSequence sequence, string fastaHeader, int? webApiId, bool complementary, bool partial, long[] alphabet, int[] building)
        {
            var parameters = FillParams(sequence, alphabet, building);
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
                ParameterName = "partial", 
                NpgsqlDbType = NpgsqlDbType.Boolean, 
                Value = partial
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "complementary", 
                NpgsqlDbType = NpgsqlDbType.Boolean,
                Value = complementary
            });

            const string Query = @"INSERT INTO dna_chain (
                                        id, 
                                        notation_id,
                                        matter_id, 
                                        feature_id, 
                                        piece_position, 
                                        fasta_header, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id, 
                                        web_api_id,
                                        partial,
                                        complementary
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id,
                                        @feature_id, 
                                        @piece_position, 
                                        @fasta_header, 
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id, 
                                        @web_api_id,
                                        @partial,
                                        @complementary
                                    );";

            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Insert(DnaSequence sequence, long[] alphabet, int[] building)
        {
            Create(ToCommonSequence(sequence), sequence.FastaHeader, sequence.WebApiId, false, false, alphabet, building);
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
        public CommonSequence ToCommonSequence(DnaSequence source)
        {
            return new CommonSequence
            {
                Id = source.Id,
                NotationId = source.NotationId, 
                MatterId = source.MatterId, 
                FeatureId = source.FeatureId, 
                PiecePosition = source.PiecePosition
            };
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="sequences">
        /// The sequences.
        /// </param>
        /// <returns>
        /// The <see cref="List{SelectListItem}"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<DnaSequence> sequences)
        {
            return GetSelectListItems(Db.DnaSequence.ToList(), sequences);
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="allSequences">
        /// The all sequences.
        /// </param>
        /// <param name="selectedSequences">
        /// The selected sequence.
        /// </param>
        /// <returns>
        /// The <see cref="List{Object}"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(
            IEnumerable<DnaSequence> allSequences,
            IEnumerable<DnaSequence> selectedSequences)
        {
            HashSet<long> sequenceIds = selectedSequences != null
                ? new HashSet<long>(selectedSequences.Select(c => c.Id))
                : new HashSet<long>();
            if (allSequences == null)
            {
                allSequences = Db.DnaSequence.Include(s => s.Matter);
            }

            var sequencesList = new List<SelectListItem>();
            foreach (var sequence in allSequences)
            {
                sequencesList.Add(new SelectListItem
                {
                    Value = sequence.Id.ToString(), 
                    Text = sequence.Matter.Name, 
                    Selected = sequenceIds.Contains(sequence.Id)
                });
            }

            return sequencesList;
        }

        /// <summary>
        /// The create complementary alphabet.
        /// </summary>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <returns>
        /// The <see cref="Alphabet"/>.
        /// </returns>
        public Alphabet CreateComplementaryAlphabet(Alphabet alphabet)
        {
            var newAlphabet = new Alphabet { NullValue.Instance() };

            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                newAlphabet.Add(GetComplementaryElement(alphabet[i]));
            }

            return newAlphabet;
        }

        /// <summary>
        /// The get complementary element.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="ValueString"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if any element in sequence is not recognized as nucleotide.
        /// </exception>
        public ValueString GetComplementaryElement(IBaseObject source)
        {
            switch (source.ToString())
            {
                case "A":
                case "a":
                    return new ValueString('T');
                case "C":
                case "c":
                    return new ValueString('G');
                case "G":
                case "g":
                    return new ValueString('C');
                case "T":
                case "t":
                    return new ValueString('A');
                default:
                    throw new ArgumentException("Unknown nucleotide.", "source");
            }
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
