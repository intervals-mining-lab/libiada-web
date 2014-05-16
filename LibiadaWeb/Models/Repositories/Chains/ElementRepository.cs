using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories
{
    using LibiadaCore.Core;
    using LibiadaCore.Core.SimpleTypes;

    public class ElementRepository : IElementRepository
    {
        private readonly LibiadaWebEntities db;

        public ElementRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public void Dispose()
        {
            db.Dispose();
        }

        public bool ElementInDb(IBaseObject element, int notationId)
        {
            String stringElement = element.ToString();
            return db.element.Any(e => e.notation_id == notationId && e.value.Equals(stringElement));
        }

        public bool ElementsInDb(Alphabet alphabet, int notationId)
        {
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                if (!ElementInDb(alphabet[i], notationId))
                {
                    return false;
                }
            }
            return true;
        }

        private void CreateLackingElements(Alphabet libiadaAlphabet, int notationId)
        {
            for (int j = 0; j < libiadaAlphabet.Cardinality; j++)
            {
                String strElem = libiadaAlphabet[j].ToString();

                if (!ElementInDb(libiadaAlphabet[j], notationId))
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
                    throw new Exception("Как минимум один из элементов создаваемого алфавита отсутствуент в БД.");
                }
            }

            var elementIds = new long[alphabet.Cardinality];
            for (int i = 0; i < alphabet.Cardinality; i++)
            {
                String stringElement = alphabet[i].ToString();
                elementIds[i] = db.element.Single(e => e.notation_id == notationId 
                                                       && e.value.Equals(stringElement)).id;
            }

            return elementIds;
        }

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

        public IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<element> allElements,
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
    }
}