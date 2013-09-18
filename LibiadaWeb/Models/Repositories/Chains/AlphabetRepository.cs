using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class AlphabetRepository : IAlphabetRepository
    {
        private readonly LibiadaWebEntities db;
        private readonly ElementRepository elementRepository;

        public AlphabetRepository(LibiadaWebEntities db)
        {
            this.db = db;
            elementRepository = new ElementRepository(db);
        }

        public IQueryable<alphabet> All
        {
            get { return db.alphabet; }
        }

        public IQueryable<alphabet> AllIncluding(params Expression<Func<alphabet, object>>[] includeProperties)
        {
            IQueryable<alphabet> query = db.alphabet;
            foreach (var includeProperty in includeProperties)
            {
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
            if (alphabet.id == default(long))
            {
                // New entity
                db.alphabet.AddObject(alphabet);
            }
            else
            {
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

        public Alphabet ToLibiadaAlphabet(long chainId)
        {
            element[] dbElements = db.alphabet.Where(a => a.chain_id == chainId).OrderBy(a => a.number)
                                    .Select(a => a.element).ToArray();

            Alphabet alphabet = new Alphabet {NullValue.Instance()};
            for (int i = 0; i < dbElements.Length; i++)
            {
                alphabet.Add(new ValueString(dbElements[i].value));
            }
            return alphabet;
        }

        public int ToDbAlphabet(Alphabet libiadaAlphabet, int notationId, long chainId,
                                                  bool createElements)
        {
            if (!createElements && !elementRepository.ElementsInDb(libiadaAlphabet, notationId))
            {
                throw new Exception("Как минимум один из элементов создаваемого алфавита отсутствуент в БД.");
            }

            elementRepository.CreateLackingElements(libiadaAlphabet, notationId);

            List<long> elementIds = new List<long>();
            for (int i = 0; i < libiadaAlphabet.Power; i++)
            {
                String stringElement = libiadaAlphabet[i].ToString();
                elementIds.Add(db.element.Single(e => e.notation_id == notationId && e.value.Equals(stringElement)).id);
            }
            String aggregatedElements = elementIds.Aggregate(new StringBuilder(), (a, b) =>
                                                                  a.Append("," + b.ToString()),
                                                                  a => a.Remove(0, 1).ToString());
            String query = "SELECT create_alphabet_from_string(" + chainId + ", '" + aggregatedElements + "')";
            return db.ExecuteStoreQuery<int>(query).First();
        }

        
    }
}