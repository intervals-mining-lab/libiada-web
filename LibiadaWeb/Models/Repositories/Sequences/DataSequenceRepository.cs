namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;
    using Npgsql;

    /// <summary>
    /// The data sequence repository.
    /// </summary>
    public class DataSequenceRepository : SequenceImporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSequenceRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public DataSequenceRepository(LibiadaWebEntities db) : base(db)
        {
        }

        /// <summary>
        /// Create data sequence and matter.
        /// </summary>
        /// <param name="sequence">
        /// The common sequence.
        /// </param>
        /// <param name="sequenceStream">
        /// The sequence stream.
        /// </param>
        /// <param name="precision">
        /// Precision of data sequence.
        /// </param>
        public void Create(CommonSequence sequence, Stream sequenceStream, int precision)
        {
            string stringSequence = FileHelper.ReadSequenceFromStream(sequenceStream);

            string[] text = stringSequence.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

           string[] cleanedSequence = text.Where(t => !t.Equals("\"volume\"") && !string.IsNullOrEmpty(t) && !string.IsNullOrWhiteSpace(t)).ToArray();

            var elements = new List<IBaseObject>(cleanedSequence.Length);

            for (int i = 0; i < cleanedSequence.Length; i++)
            {
                string element = cleanedSequence[i];
                if (element.Substring(element.Length - 2, 2).Equals(".0"))
                {
                    cleanedSequence[i] = cleanedSequence[i].Substring(0, cleanedSequence[i].Length - 2);
                }

                int intElement = int.Parse(cleanedSequence[i]);
                int multiplier = (int)Math.Pow(10, precision);
                intElement /= multiplier;
                intElement *= multiplier;

                elements.Add(new ValueInt(intElement));
            }

            var chain = new BaseChain(elements);

            MatterRepository.CreateOrExctractExistingMatterForSequence(sequence);

            long[] alphabet = ElementRepository.ToDbElements(chain.Alphabet, sequence.Notation, true);
            Create(sequence, alphabet, chain.Building);
        }

        /// <summary>
        /// Create sequence.
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
            List<NpgsqlParameter> parameters = FillParams(sequence, alphabet, building);

            const string Query = @"INSERT INTO data_chain (
                                        id,
                                        notation,
                                        matter_id,
                                        alphabet,
                                        building,
                                        remote_id,
                                        remote_db
                                    ) VALUES (
                                        @id,
                                        @notation,
                                        @matter_id,
                                        @alphabet,
                                        @building,
                                        @remote_id,
                                        @remote_db
                                    );";

            DbHelper.ExecuteCommand(Db, Query, parameters.ToArray());
        }
    }
}
