using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;
using LibiadaWeb;

namespace LibiadaWeb.Models
{ 
    public class DnaChainRepository : IDnaChainRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<dna_chain> All
        {
            get { return context.dna_chain; }
        }

        public IQueryable<dna_chain> AllIncluding(params Expression<Func<dna_chain, object>>[] includeProperties)
        {
            IQueryable<dna_chain> query = context.dna_chain;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public dna_chain Find(long id)
        {
            return context.dna_chain.Single(x => x.id == id);
        }

        public void InsertOrUpdate(dna_chain chain)
        {
            if (chain.id == default(long))
            {
                // New entity
                context.dna_chain.AddObject(chain);
            }
            else
            {
                // Existing entity
                context.dna_chain.Attach(chain);
                context.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var chain = context.dna_chain.Single(x => x.id == id);
            context.dna_chain.DeleteObject(chain);
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
            var allChains = context.dna_chain.Include("matter");
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
                if (!context.element.Any(e => e.notation_id == notationId && e.value.Equals(strElem)))
                {
                    element newElement = new element();
                    newElement.value = strElem;
                    newElement.name = strElem;
                    newElement.notation_id = notationId;
                    newElement.creation_date = DateTime.Now;
                    context.element.AddObject(newElement);
                    dbAlphabet[j].element = newElement;
                }
                else
                {
                    dbAlphabet[j].element =
                    context.element.Single(e => e.notation_id == notationId && e.value.Equals(strElem));
                }

                parent.alphabet.Add(dbAlphabet[j]); //TODO: проверить, возможно одно из действий лишнее
                context.alphabet.AddObject(dbAlphabet[j]);
            }
            context.SaveChanges();

            return dbAlphabet;
        }

        //TODO: создать репозиторий строя и перенести туда методы строя
        public int[] FromDbBuildingToLibiadaBuilding(dna_chain dbChain)
        {
            IEnumerable<building> dbBuilding = dbChain.building.OrderBy(b => b.index);
            return dbBuilding.Select(b => b.number).ToArray();
        }

        public IEnumerable<building> FromLibiadaBuildingToDbBuilding(dna_chain parent, int[] libiadaBuilding)
        {
            List<building> result = context.building.Where(b => b.chain_id == parent.id).OrderBy(b => b.index).ToList();
            int createdCount = result.Count;
            for (int i = createdCount; i < libiadaBuilding.Length; i++)
            {
                result.Add(new building());
                result[i].index = i;
                result[i].number = libiadaBuilding[i];

                parent.building.Add(result[i]); //TODO: проверить, возможно одно из действий лишнее
                context.building.AddObject(result[i]);

                //костыль чтобы БД реже умирала
                if (i % 1000 == 0)
                {
                    context.SaveChanges();
                }
            }

            context.SaveChanges();

            return result;
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void Dispose() 
        {
            context.Dispose();
        }
    }
}