namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    public class CommonSequenceRepository : SequenceImporter, ICommonSequenceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CommonSequenceRepository(LibiadaWebEntities db) : base(db)
        {
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
        public void Create(CommonSequence sequence, long[] alphabet, int[] building)
        {
            var parameters = FillParams(sequence, alphabet, building);

            const string Query = @"INSERT INTO chain (
                                        id, 
                                        notation_id,
                                        matter_id, 
                                        feature_id, 
                                        piece_position, 
                                        alphabet, 
                                        building, 
                                        remote_id, 
                                        remote_db_id
                                    ) VALUES (
                                        @id, 
                                        @notation_id,
                                        @matter_id,
                                        @feature_id, 
                                        @piece_position,
                                        @alphabet, 
                                        @building, 
                                        @remote_id, 
                                        @remote_db_id
                                    );";

            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="selectedSequences">
        /// The selected sequences.
        /// </param>
        /// <returns>
        /// The <see cref="List{SelectListItem}"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<CommonSequence> selectedSequences)
        {
            return GetSelectListItems(null, selectedSequences);
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="allSequences">
        /// The all sequences.
        /// </param>
        /// <param name="selectedSequences">
        /// The selected sequences.
        /// </param>
        /// <returns>
        /// The <see cref="List{SelectListItem}"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<CommonSequence> allSequences, IEnumerable<CommonSequence> selectedSequences)
        {
            if (allSequences == null)
            {
                allSequences = Db.CommonSequence.Include(s => s.Matter);
            }

            HashSet<long> sequenceIds = selectedSequences != null
                                          ? new HashSet<long>(selectedSequences.Select(c => c.Id))
                                          : new HashSet<long>();
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
        /// The get elements.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="List{element}"/>.
        /// </returns>
        public List<Element> GetElements(long sequenceId)
        {
            List<long> elementIds = DbHelper.GetElementIds(Db, sequenceId);
            return ElementRepository.GetElements(elementIds);
        }

        /// <summary>
        /// The get alphabet.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="Alphabet"/>.
        /// </returns>
        public Alphabet GetAlphabet(long sequenceId)
        {
            List<long> elements = DbHelper.GetElementIds(Db, sequenceId);
            return ElementRepository.ToLibiadaAlphabet(elements);
        }

        /// <summary>
        /// The to libiada BaseChain.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="BaseChain"/>.
        /// </returns>
        public BaseChain ToLibiadaBaseChain(long sequenceId)
        {
            return new BaseChain(DbHelper.GetBuilding(Db, sequenceId), GetAlphabet(sequenceId));
        }

        /// <summary>
        /// The to libiada Chain.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="Chain"/>.
        /// </returns>
        public Chain ToLibiadaChain(long sequenceId)
        {
            return new Chain(DbHelper.GetBuilding(Db, sequenceId), GetAlphabet(sequenceId));
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
