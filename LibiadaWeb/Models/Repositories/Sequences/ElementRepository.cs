namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    using LibiadaMusic.ScoreModel;

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
        private Element[] CachedElements
        {
            get
            {
                return lazyCache ?? (lazyCache = db.Element.Where(e => Aliases.Notation.StaticNotations.Contains(e.NotationId)).ToArray());
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// The elements in db.
        /// </summary>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ElementsInDb(Alphabet alphabet, int notationId)
        {
            var elements = from IBaseObject element in alphabet select element.ToString();

            int existingElementsCount = db.Element.Count(e => elements.Contains(e.Value) && e.NotationId == notationId);

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
            var result = new long[alphabet.Cardinality];
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                var note = (ValueNote)alphabet[i];
                var pitches = GetOrCreatePitchesInDb(note.Pitch);

                var localNoteHash = BitConverter.ToString(note.GetHashCode()).Replace("-", string.Empty);
                if (db.Note.Any(n => n.Value == localNoteHash))
                {
                    var databaseNote = db.Note.Single(n => n.Value == localNoteHash);
                    result[i] = databaseNote.Id;
                    if (note.Triplet != databaseNote.Triplet || note.Duration.Denominator != databaseNote.Denominator
                        || note.Duration.Numerator != databaseNote.Numerator
                        || note.Duration.Odenominator != databaseNote.Odenominator
                        || note.Duration.Onumerator != databaseNote.Onumerator || (int)note.Tie != databaseNote.TieId)
                    {
                        throw new Exception("Found in db note not equals to local note.");
                    }
                }
                else
                {
                    var newNote = new Note
                    {
                        Value = BitConverter.ToString(note.GetHashCode()).Replace("-", string.Empty),
                        Triplet = note.Triplet,
                        Denominator = note.Duration.Denominator,
                        Numerator = note.Duration.Numerator,
                        Onumerator = note.Duration.Onumerator,
                        Odenominator = note.Duration.Odenominator,
                        TieId = (int)note.Tie,
                        Priority = note.Priority,
                        Pitch = db.Pitch.Where(p => pitches.Contains(p.Id)).ToList(),
                        NotationId = Aliases.Notation.Notes
                    };
                    newNotes.Add(newNote);
                    result[i] = newNote.Id;
                }
            }

            db.Note.AddRange(newNotes);
            db.SaveChanges();
            return result;
        }

        /// <summary>
        /// The to db elements.
        /// </summary>
        /// <param name="alphabet">
        /// The alphabet.
        /// </param>
        /// <param name="notationId">
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
        public long[] ToDbElements(Alphabet alphabet, int notationId, bool createElements)
        {
            if (!ElementsInDb(alphabet, notationId))
            {
                if (createElements)
                {
                    CreateLackingElements(alphabet, notationId);
                }
                else
                {
                    throw new Exception("At least one element of alphabet is not found in database.");
                }
            }

            var staticNotation = Aliases.Notation.StaticNotations.Contains(notationId);

            var stringElements = alphabet.Select(element => element.ToString()).ToList();

            var elements = staticNotation ?
                            CachedElements.Where(e => e.NotationId == notationId && stringElements.Contains(e.Value)).ToList() :
                            db.Element.Where(e => e.NotationId == notationId && stringElements.Contains(e.Value)).ToList();

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
            foreach (long elementId in elementIds)
            {
                Element el = db.Element.Single(e => e.Id == elementId);
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
        public List<Element> GetElements(List<long> elementIds)
        {
            return db.Element.Where(e => elementIds.Contains(e.Id)).ToList();
        }

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
        public IEnumerable<SelectListItem> GetSelectListItems(
            IEnumerable<Element> allElements,
            IEnumerable<Element> selectedElements)
        {
            HashSet<long> elementIds = selectedElements != null
                                     ? new HashSet<long>(selectedElements.Select(c => c.Id))
                                     : new HashSet<long>();
            if (allElements == null)
            {
                allElements = db.Element;
            }

            var elementsList = new List<SelectListItem>();
            foreach (var element in allElements)
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
            var result = new int[pitches.Count];

            for (int i = 0; i < pitches.Count; i++)
            {
                var pitch = pitches[i];
                var noteSymbol = pitch.Step.ToString();
                var accidental = pitch.Alter.ToString();

                if (db.Pitch.Any(p => p.Midinumber == pitch.MidiNumber))
                {
                    var databasePitch = db.Pitch.Include(p => p.NoteSymbol).Include(p => p.Accidental).Single(p => p.Midinumber == pitch.MidiNumber);
                    result[i] = databasePitch.Id;

                    if (pitch.Alter.ToString() != databasePitch.Accidental.Name
                        || pitch.Step.ToString() != databasePitch.NoteSymbol.Name
                        || pitch.Octave != databasePitch.Octave)
                    {
                        throw new Exception("Found in db pitch not equals to local pitch.");
                    }
                }
                else
                {
                    var newPitch = new LibiadaWeb.Pitch
                    {
                        AccidentalId = db.Accidental.Single(a => a.Name == accidental).Id,
                        NoteSymbolId = db.NoteSymbol.Single(n => n.Name == noteSymbol).Id,
                        Octave = pitch.Octave,
                        Midinumber = pitch.MidiNumber
                    };
                    newPitches.Add(newPitch);
                    result[i] = newPitch.Id;
                }
            }

            db.Pitch.AddRange(newPitches);
            db.SaveChanges();
            return result;
        }

        /// <summary>
        /// The create lacking elements.
        /// </summary>
        /// <param name="libiadaAlphabet">
        /// The libiada alphabet.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        private void CreateLackingElements(Alphabet libiadaAlphabet, int notationId)
        {
            var newElements = new List<Element>();
            List<string> elements = (from IBaseObject element in libiadaAlphabet select element.ToString()).ToList();

            var existingElements = db.Element.Where(e => elements.Contains(e.Value) && e.NotationId == notationId).Select(e => e.Value);

            var notExistingElements = elements.Where(e => !existingElements.Contains(e)).ToList();

            foreach (var element in notExistingElements)
            {
                var newElement = new Element
                {
                    Value = element,
                    Name = element,
                    NotationId = notationId
                };
                newElements.Add(newElement);
            }

            db.Element.AddRange(newElements);
            db.SaveChanges();
        }
    }
}
