namespace LibiadaWeb.Models.Repositories.Sequences
{
    using SixLabors.ImageSharp;

    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;

    using LibiadaCore.Core;
    using LibiadaCore.Extensions;
    using LibiadaCore.Music;
    using LibiadaCore.Images;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Attributes;
    using LibiadaWeb.Extensions;


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
            var imageSequence = Db.ImageSequences.Include(s => s.Matter).Single(s => s.Id == sequenceId);
            if (imageSequence.Matter.Nature != Nature.Image)
            {
                throw new Exception("Cannot find sequence to return");
            }

            var image = Image.Load(imageSequence.Matter.Source);
            var orderExtractor = imageSequence.OrderExtractor.GetAttribute<ImageOrderExtractor, ImageOrderExtractorAttribute>().Value;
            var sequence = ImageProcessor.ProcessImage(image, new IImageTransformer[0], new IMatrixTransformer[0], (IImageOrderExtractor)Activator.CreateInstance(orderExtractor));
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
            Language?[] languages,
            Translator?[] translators,
            PauseTreatment?[] pauseTreatments,
            bool?[] sequentialTransfers,
            ImageOrderExtractor?[] imageOrderExtractors)
        {


            var sequenceIds = new long[matterIds.Length][];
            CreateMissingImageSequences(matterIds, notations, imageOrderExtractors);
            for (int i = 0; i < matterIds.Length; i++)
            {
                sequenceIds[i] = new long[notations.Length];
            }
            for (int j = 0; j < notations.Length; j++)
            {
                var sequenceIdsForOneNotation = GetSequenceIds(matterIds,
                                                               notations[j],
                                                               languages?[j],
                                                               translators?[j],
                                                               pauseTreatments?[j],
                                                               sequentialTransfers?[j],
                                                               imageOrderExtractors?[j]);
                for(int i = 0; i < matterIds.Length; i++)
                {
                    sequenceIds[i][j] = sequenceIdsForOneNotation[i];

                }
            }

            return sequenceIds;
        }

        public long[] GetSequenceIds(long[] matterIds,
            Notation notation,
            Language? language,
            Translator? translator,
            PauseTreatment? pauseTreatment,
            bool? sequentialTransfer,
            ImageOrderExtractor? imageOrderExtractor)
        {
            var sequenceIds = new long[matterIds.Length];
            for (int i = 0; i < matterIds.Length; i++)
            {
                switch (notation.GetNature())
                {
                    case Nature.Literature:
                        return Db.LiteratureSequence
                                 .Where(l => matterIds.Contains(l.MatterId)
                                          && l.Notation == notation
                                          && l.Language == language
                                          && l.Translator == translator)
                                 .ToArray()
                                 .OrderBy(s => { return Array.IndexOf(matterIds, s.MatterId); })
                                 .Select(s => s.Id)
                                 .ToArray();
                    case Nature.Music:
                        return Db.MusicSequence
                                 .Where(m => matterIds.Contains(m.MatterId)
                                          && m.Notation == notation
                                          && m.PauseTreatment == pauseTreatment
                                          && m.SequentialTransfer == sequentialTransfer)
                                 .ToArray()
                                 .OrderBy(s => { return Array.IndexOf(matterIds, s.MatterId); })
                                 .Select(s => s.Id)
                                 .ToArray();
                    case Nature.Image:
                        return Db.ImageSequences
                                 .Where(c => matterIds.Contains(c.MatterId)
                                          && c.Notation == notation
                                          && c.OrderExtractor == imageOrderExtractor)
                                 .ToArray()
                                 .OrderBy(s => { return Array.IndexOf(matterIds, s.MatterId); })
                                 .Select(s => s.Id)
                                 .ToArray();
                    default:
                        return Db.CommonSequence
                                 .Where(c => matterIds.Contains(c.MatterId) && c.Notation == notation)
                                 .ToArray()
                                 .OrderBy(s => { return Array.IndexOf(matterIds, s.MatterId); })
                                 .Select(s => s.Id)
                                 .ToArray();
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

        /// <summary>
        /// Creates image sequences for given order reading trajectories
        /// if they are missing in database.
        /// </summary>
        /// <param name="matterIds">
        /// Matters ids.
        /// </param>
        /// <param name="notations">
        /// notations of images.
        /// </param>
        /// <param name="imageOrderExtractors">
        /// Reading trajectories.
        /// </param>
        private void CreateMissingImageSequences(long[] matterIds, Notation[] notations, ImageOrderExtractor?[] imageOrderExtractors)
        {
            if (notations[0].GetNature() == Nature.Image)
            {
                var existingSequences = Db.ImageSequences
                    .Where(s => matterIds.Contains(s.MatterId)
                             && notations.Contains(s.Notation)
                             && imageOrderExtractors.Contains(s.OrderExtractor))
                    .ToList();

                ImageSequenceRepository imageSequenceRepository = new ImageSequenceRepository();
                for (int i = 0; i < matterIds.Length; i++)
                {
                    for (int j = 0; j < notations.Length; j++)
                    {
                        if (!existingSequences.Any(s => s.MatterId == matterIds[i]
                                                     && s.Notation == notations[j]
                                                     && s.OrderExtractor == imageOrderExtractors[j]))
                        {
                            var newImageSequence = new ImageSequence()
                            {
                                MatterId = matterIds[i],
                                Notation = notations[j],
                                OrderExtractor = imageOrderExtractors[j]??ImageOrderExtractor.LineLeftToRightTopToBottom
                            };
                            imageSequenceRepository.Create(newImageSequence, Db);
                            existingSequences.Add(newImageSequence);
                        }
                    }
                }

                Db.SaveChanges();
            }
        }
    }
}
