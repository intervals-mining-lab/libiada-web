namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using SixLabors.ImageSharp;
    using System.Linq;
    using System.Text;

    using LibiadaCore.Core;
    using LibiadaCore.Music;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaCore.Images;
    using LibiadaCore.Core.SimpleTypes;
    using System;


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

            Db.ExecuteCommand(Query, parameters.ToArray());
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
            long[] elementIds = Db.GetAlphabetElementIds(sequenceId);
            return ElementRepository.GetElements(elementIds);
        }

        /// <summary>
        /// Loads sequence by id from db and converts it to <see cref="BaseChain"/>.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The sequence as <see cref="BaseChain"/>.
        /// </returns>
        public BaseChain GetLibiadaBaseChain(long sequenceId)
        {
            return new BaseChain(Db.GetSequenceBuilding(sequenceId), GetAlphabet(sequenceId), sequenceId);
        }

        /// <summary>
        /// Loads sequence by id from db and converts it to <see cref="Chain"/>.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The sequence as <see cref="Chain"/>.
        /// </returns>
        public Chain GetLibiadaChain(long sequenceId)
        {
           
            if (Db.CommonSequence.Any(s => s.Id == sequenceId))
            {
                var matter = Db.CommonSequence.Include(s => s.Matter).Single(s => s.Id == sequenceId).Matter;
                return new Chain(Db.GetSequenceBuilding(sequenceId), GetAlphabet(sequenceId), sequenceId);
            }

            // if it is not "real" sequence , then it must be image "sequence" 
            var imageMatter = Db.ImageSequences.Include(s => s.Matter).Single(s => s.Id == sequenceId).Matter;
            if (imageMatter.Nature != Nature.Image)
            {
                throw new Exception("Cannot find sequence to return");
            }

            var image = Image.Load(imageMatter.Source);
            var sequence = ImageProcessor.ProcessImage(image, new IImageTransformer[0], new IMatrixTransformer[0], new LineOrderExtractor());
            var alphabet = new Alphabet { NullValue.Instance() };
            var incompleteAlphabet = sequence.Alphabet;
            for (int j = 0; j < incompleteAlphabet.Cardinality; j++)
            {
                alphabet.Add(incompleteAlphabet[j]);
            }

            return new Chain(sequence.Building, alphabet);
        }
        
        /// <summary>
        /// Loads sequence by id from db and converts it to <see cref="BaseChain"/>.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The sequence as <see cref="BaseChain"/>.
        /// </returns>
        public string GetString(long sequenceId)
        {
            int[] order = Db.GetSequenceBuilding(sequenceId);
            Alphabet alphabet = GetAlphabet(sequenceId);
            var stringBuilder = new StringBuilder(order.Length);
            foreach (int element in order)
            {
                stringBuilder.Append(alphabet[element]);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Extracts sequences ids from database.
        /// </summary>
        /// <param name="matterIds">
        /// The matters ids.
        /// </param>
        /// <param name="notations">
        /// The notations ids.
        /// </param>
        /// <param name="languages">
        /// The languages ids.
        /// </param>
        /// <param name="translators">
        /// The translators ids.
        /// </param>
        /// <param name="pauseTreatments">
        /// Pause treatment parameters of music sequences.
        /// </param>
        /// <param name="sequentialTransfers">
        /// Sequential transfer flag used in music sequences.
        /// </param>
        /// <returns>
        /// The sequences ids as <see cref="T:long[][]"/>.
        /// </returns>
        public long[][] GetSequenceIds(
            long[] matterIds,
            Notation[] notations,
            Language[] languages,
            Translator?[] translators,
            PauseTreatment[] pauseTreatments,
            bool[] sequentialTransfers,
            ImageOrderExtractor imageOrderExtractor)
        {
            var sequenceIds = new long[matterIds.Length][];

            for (int i = 0; i < matterIds.Length; i++)
            {
                var matterId = matterIds[i];
                sequenceIds[i] = new long[notations.Length];

                for (int j = 0; j < notations.Length; j++)
                {
                    Notation notation = notations[j];

                    switch (notation.GetNature())
                    {
                        case Nature.Literature:
                            Language language = languages[j];
                            Translator translator = translators[j] ?? Translator.NoneOrManual;
                            sequenceIds[i][j] = Db.LiteratureSequence.Single(l => l.MatterId == matterId
                                                                                  && l.Notation == notation
                                                                                  && l.Language == language
                                                                                  && l.Translator == translator).Id;
                            break;
                        case Nature.Music:
                            PauseTreatment pauseTreatment = pauseTreatments[j];
                            bool sequentialTransfer = sequentialTransfers[j];
                            sequenceIds[i][j] = Db.MusicSequence.Single(m => m.MatterId == matterId
                                                                          && m.Notation == notation
                                                                          && m.PauseTreatment == pauseTreatment
                                                                          && m.SequentialTransfer == sequentialTransfer).Id;
                            break;
                        case Nature.Image:
                            sequenceIds[i][j] = Db.ImageSequences.Single(c => c.MatterId == matterId && c.Notation == notation && c.OrderExtractor == imageOrderExtractor).Id;
                            break;
                        default:
                            sequenceIds[i][j] = Db.CommonSequence.Single(c => c.MatterId == matterId && c.Notation == notation).Id;
                            break;
                    }

                }
            }

            return sequenceIds;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
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
        private Alphabet GetAlphabet(long sequenceId)
        {
            long[] elements = Db.GetAlphabetElementIds(sequenceId);
            return ElementRepository.ToLibiadaAlphabet(elements);
        }
    }
}
