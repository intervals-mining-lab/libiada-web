using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;

namespace LibiadaWeb.Models
{ 
    public class DnaChainRepository : IDnaChainRepository
    {
        private readonly LibiadaWebEntities db;

        public DnaChainRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<dna_chain> All
        {
            get { return db.dna_chain; }
        }

        public IQueryable<dna_chain> AllIncluding(params Expression<Func<dna_chain, object>>[] includeProperties)
        {
            IQueryable<dna_chain> query = db.dna_chain;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public dna_chain Find(long id)
        {
            return db.dna_chain.Single(x => x.id == id);
        }

        public void InsertOrUpdate(dna_chain chain)
        {
            if (chain.id == default(long))
            {
                // New entity
                db.dna_chain.AddObject(chain);
            }
            else
            {
                // Existing entity
                db.dna_chain.Attach(chain);
                db.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var chain = db.dna_chain.Single(x => x.id == id);
            db.dna_chain.DeleteObject(chain);
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<dna_chain> chains)
        {
            HashSet<long> chainIds;
            if (chains != null)
            {
                chainIds = new HashSet<long>(chains.Select(c => c.id));
            }
            else
            {
                chainIds = new HashSet<long>();
            }
            var allChains = db.dna_chain.Include("matter");
            var chainsList = new List<SelectListItem>();
            foreach (var chain in allChains)
            {
                chainsList.Add(new SelectListItem
                {
                    Value = chain.id.ToString(),
                    Text = chain.matter.name,
                    Selected = chainIds.Contains(chain.id)
                });
            }
            return chainsList;
        }

        //TODO: сделать в таблице алфавита id чтобы можно было создать репозиторий алфавита и переместить туда методы алфавита
        public Alphabet FromDbAlphabetToLibiadaAlphabet(dna_chain dbChain)
        {
            IEnumerable<alphabet> dbAlphabet = dbChain.alphabet.OrderBy(a => a.number);
            IEnumerable<element> dbElements = dbAlphabet.Select(a => a.element);

            Alphabet alphabet = new Alphabet();
            alphabet.Add(NullValue.Instance());
            foreach (var element in dbElements)
            {
                alphabet.Add(new ValueString(element.value));
            }
            return alphabet;
        }

        public IEnumerable<alphabet> FromLibiadaAlphabetToDbAlphabet(Alphabet libiadaAlphabet, dna_chain parent,
                                                                     int notationId)
        {
            List<alphabet> dbAlphabet = new List<alphabet>();
            for (int j = 0; j < libiadaAlphabet.Power; j++)
            {
                dbAlphabet.Add(new alphabet());
                dbAlphabet[j].number = j + 1;
                String strElem = libiadaAlphabet[j].ToString();
                if (!db.element.Any(e => e.notation_id == notationId && e.value.Equals(strElem)))
                {
                    element newElement = new element();
                    newElement.value = strElem;
                    newElement.name = strElem;
                    newElement.notation_id = notationId;
                    newElement.creation_date = DateTime.Now;
                    db.element.AddObject(newElement);
                    dbAlphabet[j].element = newElement;
                }
                else
                {
                    dbAlphabet[j].element =
                    db.element.Single(e => e.notation_id == notationId && e.value.Equals(strElem));
                }

                parent.alphabet.Add(dbAlphabet[j]); //TODO: проверить, возможно одно из действий лишнее
                db.alphabet.AddObject(dbAlphabet[j]);
            }
            db.SaveChanges();

            return dbAlphabet;
        }

        //TODO: создать репозиторий строя и перенести туда методы строя
        public int[] FromDbBuildingToLibiadaBuilding(dna_chain dbChain)
        {
            String query = "SELECT number FROM building WHERE chain_id = " + dbChain.id + " ORDER BY index";
            return db.ExecuteStoreQuery<int>(query).ToArray();
        }

        public IEnumerable<building> FromLibiadaBuildingToDbBuilding(dna_chain parent, int[] libiadaBuilding)
        {
            List<building> result = db.building.Where(b => b.chain_id == parent.id).OrderBy(b => b.index).ToList();
            int createdCount = result.Count;
            for (int i = createdCount; i < libiadaBuilding.Length; i++)
            {
                result.Add(new building());
                result[i].index = i;
                result[i].number = libiadaBuilding[i];

                parent.building.Add(result[i]); //TODO: проверить, возможно одно из действий лишнее
                db.building.AddObject(result[i]);

                //костыль чтобы БД реже умирала
                if (i % 1000 == 0)
                {
                    db.SaveChanges();
                }
            }

            db.SaveChanges();

            return result;
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose() 
        {
            db.Dispose();
        }
    }
}