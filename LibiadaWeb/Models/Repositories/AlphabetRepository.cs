using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;

namespace LibiadaWeb.Models.Repositories
{ 
    public class AlphabetRepository : IAlphabetRepository
    {
        private readonly LibiadaWebEntities db;

        public AlphabetRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<alphabet> All
        {
            get { return db.alphabet; }
        }

        public IQueryable<alphabet> AllIncluding(params Expression<Func<alphabet, object>>[] includeProperties)
        {
            IQueryable<alphabet> query = db.alphabet;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public alphabet Find(long id)
        {
            return db.alphabet.Single(x => x.id == id);
        }

        public void InsertOrUpdate(alphabet alphabet)
        {
            if (alphabet.id == default(long)) {
                // New entity
                db.alphabet.AddObject(alphabet);
            } else {
                // Existing entity
                db.alphabet.Attach(alphabet);
                db.ObjectStateManager.ChangeObjectState(alphabet, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var alphabet = Find(id);
            db.alphabet.DeleteObject(alphabet);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose() 
        {
            db.Dispose();
        }


        public Alphabet FromDbAlphabetToLibiadaAlphabet(long chainId)
        {
            IEnumerable<alphabet> alphabet = db.alphabet.Where(a => a.chain_id == chainId).OrderBy(a => a.number);
            return FromDbAlphabetToLibiadaAlphabet(alphabet);
        }
        
        public Alphabet FromDbAlphabetToLibiadaAlphabet(IEnumerable<alphabet> dbAlphabet)
        {
            IEnumerable<element> dbElements = dbAlphabet.Select(a => a.element);

            Alphabet alphabet = new Alphabet {NullValue.Instance()};
            foreach (var element in dbElements)
            {
                alphabet.Add(new ValueString(element.value));
            }
            return alphabet;
        }

        public bool CheckAlphabetElementsInDb(Alphabet alphabet, int notationId)
        {
            for (int i = 0; i < alphabet.Power; i++)
            {
                String strElement = alphabet[i].ToString();
                if (!db.element.Any(e => e.notation_id == notationId && e.value.Equals(strElement)))
                {
                    return false;
                }
            }
            return true;
        } 

        public IEnumerable<alphabet> FromLibiadaAlphabetToDbAlphabet(Alphabet libiadaAlphabet, int notationId, long chainId, bool createElements)
        {
            List<alphabet> dbAlphabet = new List<alphabet>();
            for (int j = 0; j < libiadaAlphabet.Power; j++)
            {
                dbAlphabet.Add(new alphabet());
                dbAlphabet[j].number = j + 1;
                String strElem = libiadaAlphabet[j].ToString();

                if (!createElements && !db.element.Any(e => e.notation_id == notationId && e.value.Equals(strElem)))
                {
                    throw new Exception("Ёлемент " + strElem + " не найден в Ѕƒ.");
                }

                if (!db.element.Any(e => e.notation_id == notationId && e.value.Equals(strElem)))
                {
                    element newElement = new element
                        {
                        value = strElem,
                        name = strElem,
                        notation_id = notationId,
                        creation_date = DateTime.Now
                    };
                    db.element.AddObject(newElement);
                    dbAlphabet[j].element = newElement;
                }
                else
                {
                    dbAlphabet[j].element =
                        db.element.Single(e => e.notation_id == notationId && e.value.Equals(strElem));
                }
                dbAlphabet[j].chain_id = chainId;
                db.alphabet.AddObject(dbAlphabet[j]);
                db.SaveChanges();
            }

            return dbAlphabet;
        }
    }
}