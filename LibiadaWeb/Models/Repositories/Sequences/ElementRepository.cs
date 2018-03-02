namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    /// <summary>
    /// The element repository.
    /// </summary>
    public class ElementRepository : IElementRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The lazy cache.
        /// </summary>
        private Element[] lazyCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public ElementRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// Gets the cached elements.
        /// </summary>
        private Element[] CachedElements => lazyCache ?? (lazyCache = db.Element.Where(e => Aliases.StaticNotations.Contains(e.Notation)).ToArray());

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// The elements in db.
        /// </summary>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ElementsInDb(Alphabet alphabet, Notation notation)
        {
            IEnumerable<string> elements = alphabet.Select(e => e.ToString());

            int existingElementsCount = db.Element.Count(e => elements.Contains(e.Value) && e.Notation == notation);

            return alphabet.Cardinality == existingElementsCount;
        }

        /// <summary>
        /// The get or create notes in db.
        /// </summary>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <returns>
        /// The <see cref="T:long[]"/>.
        /// </returns>
        public long[] GetOrCreateNotesInDb(Alphabet alphabet)
        {
            var newNotes = new List<Note>();
            var result = new Note[alphabet.Cardinality];
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                var note = (ValueNote)alphabet[i];
                int[] pitches = GetOrCreatePitchesInDb(note.Pitches);

                string localNoteHash = BitConverter.ToString(note.GetMD5HashCode()).Replace("-", string.Empty);
                if (db.Note.Any(n => n.Value == localNoteHash))
                {
                    result[i] = db.Note.Single(n => n.Value == localNoteHash);
                    if (note.Triplet != result[i].Triplet
                     || note.Duration.Denominator != result[i].Denominator
                     || note.Duration.Numerator != result[i].Numerator
                     || note.Duration.OriginalDenominator != result[i].Odenominator
                     || note.Duration.OriginalNumerator != result[i].Onumerator
                     || note.Tie != result[i].Tie)
                    {
                        throw new Exception("Found in db note is not equal to local note.");
                    }
                }
                else
                {
                    result[i] = new Note
                    {
                        Value = BitConverter.ToString(note.GetMD5HashCode()).Replace("-", string.Empty),
                        Triplet = note.Triplet,
                        Denominator = note.Duration.Denominator,
                        Numerator = note.Duration.Numerator,
                        Onumerator = note.Duration.OriginalNumerator,
                        Odenominator = note.Duration.OriginalDenominator,
                        Tie = note.Tie,
                        Priority = note.Priority,
                        Pitch = db.Pitch.Where(p => pitches.Contains(p.Id)).ToList(),
                        Notation = Notation.Notes
                    };
                    newNotes.Add(result[i]);
                }
            }

            db.Note.AddRange(newNotes);
            db.SaveChanges();
            return result.Select(n => n.Id).ToArray();
        }

        /// <summary>
        /// The to db elements.
        /// </summary>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <param name="createElements">
        /// The create elements.
        /// </param>
        /// <returns>
        /// The <see cref="T:long[]"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if alphabet element is not found in db.
        /// </exception>
        public long[] ToDbElements(Alphabet alphabet, Notation notation, bool createElements)
        {
            if (!ElementsInDb(alphabet, notation))
            {
                if (createElements)
                {
                    CreateLackingElements(alphabet, notation);
                }
                else
                {
                    throw new Exception("At least one element of alphabet is not found in database.");
                }
            }

            bool staticNotation = Aliases.StaticNotations.Contains(notation);

            List<string> stringElements = alphabet.Select(element => element.ToString()).ToList();

            List<Element> elements = staticNotation ?
                            CachedElements.Where(e => e.Notation == notation && stringElements.Contains(e.Value)).ToList() :
                            db.Element.Where(e => e.Notation == notation && stringElements.Contains(e.Value)).ToList();

            return (from stringElement in stringElements
                    join element in elements
                    on stringElement equals element.Value
                    select element.Id).ToArray();
        }

        /// <summary>
        /// The to libiada alphabet.
        /// </summary>
        /// <param name="elementIds">
        /// The element ids.
        /// </param>
        /// <returns>
        /// The <see cref="Alphabet"/>.
        /// </returns>
        public Alphabet ToLibiadaAlphabet(List<long> elementIds)
        {
            var alphabet = new Alphabet { NullValue.Instance() };
            List<Element> elements = GetElements(elementIds);
            foreach (long elementId in elementIds)
            {
                Element el = elements.Single(e => e.Id == elementId);
                alphabet.Add(new ValueString(el.Value));
            }

            return alphabet;
        }

        /// <summary>
        /// The get elements.
        /// </summary>
        /// <param name="elementIds">
        /// The element ids.
        /// </param>
        /// <returns>
        /// The <see cref="List{Element}"/>.
        /// </returns>
        public List<Element> GetElements(List<long> elementIds) => db.Element
                                                                     .Where(e => elementIds.Contains(e.Id))
                                                                     .ToList()
                                                                     .OrderBy(e => elementIds.IndexOf(e.Id))
                                                                     .ToList();

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="allElements">
        /// The all elements.
        /// </param>
        /// <param name="selectedElements">
        /// The selected elements.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{SelectListItem}"/>.
        /// </returns>
        public IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<Element> allElements, IEnumerable<Element> selectedElements)
        {
            HashSet<long> elementIds = selectedElements != null
                                     ? new HashSet<long>(selectedElements.Select(c => c.Id))
                                     : new HashSet<long>();
            if (allElements == null)
            {
                allElements = db.Element;
            }

            var elementsList = new List<SelectListItem>();
            foreach (Element element in allElements)
            {
                elementsList.Add(new SelectListItem
                    {
                        Value = element.Id.ToString(),
                        Text = element.Name,
                        Selected = elementIds.Contains(element.Id)
                    });
            }

            return elementsList;
        }

        /// <summary>
        /// The get or create pitches in db.
        /// </summary>
        /// <param name="pitches">
        /// The pitches.
        /// </param>
        /// <returns>
        /// The <see cref="T:int[]"/>.
        /// </returns>
        private int[] GetOrCreatePitchesInDb(List<Pitch> pitches)
        {
            var newPitches = new List<LibiadaWeb.Pitch>();
            var result = new LibiadaWeb.Pitch[pitches.Count];

            for (int i = 0; i < pitches.Count; i++)
            {
                Pitch pitch = pitches[i];

                if (db.Pitch.Any(p => p.Midinumber == pitch.MidiNumber))
                {
                    result[i] = db.Pitch.Single(p => p.Midinumber == pitch.MidiNumber);

                    if (pitch.Alter != result[i].Accidental
                     || pitch.Step != result[i].NoteSymbol
                     || pitch.Octave != result[i].Octave)
                    {
                        throw new Exception("Found in db pitch is not equal to the local pitch.");
                    }
                }
                else
                {
                    result[i] = new LibiadaWeb.Pitch
                    {
                        Accidental = pitch.Alter,
                        NoteSymbol = pitch.Step,
                        Octave = pitch.Octave,
                        Midinumber = pitch.MidiNumber
                    };
                    newPitches.Add(result[i]);
                }
            }

            db.Pitch.AddRange(newPitches);
            db.SaveChanges();
            return result.Select(p => p.Id).ToArray();
        }

        /// <summary>
        /// The create lacking elements.
        /// </summary>
        /// <param name="libiadaAlphabet">
        /// The libiada alphabet.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        private void CreateLackingElements(Alphabet libiadaAlphabet, Notation notation)
        {
            var newElements = new List<Element>();
            List<string> elements = (from IBaseObject element in libiadaAlphabet select element.ToString()).ToList();

            IQueryable<string> existingElements = db.Element.Where(e => elements.Contains(e.Value) && e.Notation == notation).Select(e => e.Value);

            List<string> notExistingElements = elements.Where(e => !existingElements.Contains(e)).ToList();

            foreach (string element in notExistingElements)
            {
                var newElement = new Element
                {
                    Value = element,
                    Name = element,
                    Notation = notation
                };
                newElements.Add(newElement);
            }

            db.Element.AddRange(newElements);
            db.SaveChanges();
        }
    }
}
