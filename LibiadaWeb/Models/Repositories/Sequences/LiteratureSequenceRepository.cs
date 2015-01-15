namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    /// The literature sequence repository.
    /// </summary>
    public class LiteratureSequenceRepository : SequenceImporter, ILiteratureSequenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteratureSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public LiteratureSequenceRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// The create literature sequence.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="translatorId">
        /// The translator id.
        /// </param>
        /// <param name="stringSequence">
        /// The string sequence.
        /// </param>
        public void Create(CommonSequence commonSequence, int languageId, bool original, int? translatorId, string stringSequence)
        {
            string[] text = stringSequence.Split('\n');
            for (int l = 0; l < text.Length - 1; l++)
            {
                // убираем \r
                text[l] = text[l].Substring(0, text[l].Length - 1);
            }

            var chain = new BaseChain(text.Length - 1);

            // в конце файла всегда пустая строка поэтому последний элемент не считаем
            // TODO: переделать этот говнокод и вообще добавить проверку на пустую строку в конце, а лучше сделать нормальный trim
            for (int i = 0; i < text.Length - 1; i++)
            {
                chain.Set(new ValueString(text[i]), i);
            }

            MatterRepository.CreateMatterFromSequence(commonSequence);

            var alphabet = ElementRepository.ToDbElements(chain.Alphabet, commonSequence.NotationId, true);
            Create(commonSequence, original, languageId, translatorId, alphabet, chain.Building);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="translatorId">
        /// The translator id.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="building">
        /// The building.
        /// </param>
        public void Create(CommonSequence sequence, bool original, int languageId, int? translatorId, long[] alphabet, int[] building)
        {
            var parameters = FillParams(sequence, alphabet, building);

            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "original", 
                NpgsqlDbType = NpgsqlDbType.Boolean, 
                Value = original
            });
            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "language_id", 
                NpgsqlDbType = NpgsqlDbType.Integer, 
                Value = languageId
            });

            parameters.Add(new NpgsqlParameter
            {
                ParameterName = "translator_id", 
                NpgsqlDbType = NpgsqlDbType.Integer, 
                Value = translatorId
            });

            const string Query = @"INSERT INTO literature_chain (
                                        id, 
                                        notation_id,
                                        matter_id,
                                        piece_type_id, 
                                        piece_position, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id,
                                        original,
                                        language_id,
                                        translator_id
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id,
                                        @piece_type_id, 
                                        @piece_position, 
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id,
                                        @original,
                                        @language_id,
                                        @translator_id
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
        public CommonSequence ToCommonSequence(LiteratureSequence source)
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
        /// The get select list items.
        /// </summary>
        /// <param name="sequences">
        /// The sequences.
        /// </param>
        /// <returns>
        /// The <see cref="List{SelectListItem}"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<LiteratureSequence> sequences)
        {
            HashSet<long> sequenceIds = sequences != null
                                         ? new HashSet<long>(sequences.Select(c => c.Id))
                                         : new HashSet<long>();
            var allSequences = Db.LiteratureSequence.Include(s => s.Matter);
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
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            Db.Dispose();
        }
    }
}
