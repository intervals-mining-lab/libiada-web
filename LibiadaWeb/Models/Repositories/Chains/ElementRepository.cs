namespace LibiadaWeb.Models.Repositories.Chains
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
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// The element in db.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ElementInDb(IBaseObject element, int notationId)
        {
            string stringElement = element.ToString();
            return db.element.Any(e => e.notation_id == notationId && e.value.Equals(stringElement));
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
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                if (!this.ElementInDb(alphabet[i], notationId))
                {
                    return false;
                }
            }

            return true;
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
        /// The <see cref="long[]"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public long[] ToDbElements(Alphabet alphabet, int notationId, bool createElements)
        {
            if (!this.ElementsInDb(alphabet, notationId))
            {
                if (createElements)
                {
                    this.CreateLackingElements(alphabet, notationId);
                }
                else
                {
                    throw new Exception("Как минимум один из элементов создаваемого алфавита отсутствуент в БД.");
                }
            }

            var elementIds = new long[alphabet.Cardinality];
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                string stringElement = alphabet[i].ToString();
                elementIds[i] = db.element.Single(e => e.notation_id == notationId 
                                                       && e.value.Equals(stringElement)).id;
            }

            return elementIds;
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
                element el = db.element.Single(e => e.id == elementId);
                alphabet.Add(new ValueString(el.value));
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
        /// The <see cref="List"/>.
        /// </returns>
        public List<element> GetElements(List<long> elementIds)
        {
            var elements = new List<element>();
            for (int i = 0; i < elementIds.Count(); i++)
            {
                long elementId = elementIds[i];
                elements.Add(db.element.Single(e => e.id == elementId));
                
            }

            return elements;
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
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<SelectListItem> GetSelectListItems(
            IEnumerable<element> allElements,
            IEnumerable<element> selectedElements)
        {
            HashSet<long> elementIds = selectedElements != null
                                     ? new HashSet<long>(selectedElements.Select(c => c.id))
                                     : new HashSet<long>();
            if (allElements == null)
            {
                allElements = db.element;
            }

            var elementsList = new List<SelectListItem>();
            foreach (var element in allElements)
            {
                elementsList.Add(new SelectListItem
                    {
                        Value = element.id.ToString(), 
                        Text = element.name, 
                        Selected = elementIds.Contains(element.id)
                    });
            }

            return elementsList;
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="elements">
        /// The elements.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<element> elements)
        {
            HashSet<long> elementIds = elements != null
                                           ? new HashSet<long>(elements.Select(c => c.id))
                                           : new HashSet<long>();
            var allElements = db.element;
            var elementsList = new List<SelectListItem>();
            foreach (var element in allElements)
            {
                elementsList.Add(new SelectListItem
                    {
                        Value = element.id.ToString(), 
                        Text = element.name, 
                        Selected = elementIds.Contains(element.id)
                    });
            }

            return elementsList;
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
            for (int j = 0; j < libiadaAlphabet.Cardinality; j++)
            {
                string strElem = libiadaAlphabet[j].ToString();

                if (!this.ElementInDb(libiadaAlphabet[j], notationId))
                {
                    var newElement = new element
                    {
                        value = strElem,
                        name = strElem,
                        notation_id = notationId,
                        created = DateTime.Now
                    };
                    db.element.Add(newElement);
                }
            }

            db.SaveChanges();
        }
    }
}