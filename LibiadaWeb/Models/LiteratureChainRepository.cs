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
    public class LiteratureChainRepository : ILiteratureChainRepository
    {
        private LibiadaWebEntities context;

        public LiteratureChainRepository(LibiadaWebEntities db)
        {
            context = db;
        }

        public IQueryable<literature_chain> All
        {
            get { return context.literature_chain; }
        }

        public IQueryable<literature_chain> AllIncluding(
            params Expression<Func<literature_chain, object>>[] includeProperties)
        {
            IQueryable<literature_chain> query = context.literature_chain;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public literature_chain Find(long id)
        {
            return context.literature_chain.Single(x => x.id == id);
        }

        public void InsertOrUpdate(literature_chain literature_chain)
        {
            if (literature_chain.id == default(long))
            {
                // New entity
                context.literature_chain.AddObject(literature_chain);
            }
            else
            {
                // Existing entity
                context.literature_chain.Attach(literature_chain);
                context.ObjectStateManager.ChangeObjectState(literature_chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var literature_chain = context.literature_chain.Single(x => x.id == id);
            context.literature_chain.DeleteObject(literature_chain);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<literature_chain> chains)
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
            var allChains = context.literature_chain.Include("matter");
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
        public Alphabet FromDbAlphabetToLibiadaAlphabet(literature_chain dbChain)
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

        public IEnumerable<alphabet> FromLibiadaAlphabetToDbAlphabet(Alphabet libiadaAlphabet, literature_chain parent,
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
                    element newElement = new element()
                                             {
                                                 value = strElem,
                                                 name = strElem,
                                                 notation_id = notationId,
                                                 creation_date = DateTime.Now
                                             };
                    context.element.AddObject(newElement);
                    context.SaveChanges();
                    dbAlphabet[j].element = newElement;
                }
                else
                {
                    dbAlphabet[j].element =
                    context.element.Single(e => e.notation_id == notationId && e.value.Equals(strElem));
                }

                parent.alphabet.Add(dbAlphabet[j]); //TODO: проверить, возможно одно из действий лишнее
                context.alphabet.AddObject(dbAlphabet[j]);
                context.SaveChanges();
            }
            

            return dbAlphabet;
        }

        //TODO: создать репозиторий строя и перенести туда методы строя
        public int[] FromDbBuildingToLibiadaBuilding(literature_chain dbChain)
        {
            IEnumerable<building> dbBuilding = dbChain.building.OrderBy(b => b.index);
            return dbBuilding.Select(b => b.number).ToArray();
        }

        public IEnumerable<building> FromLibiadaBuildingToDbBuilding(literature_chain parent, int[] libiadaBuilding)
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
                if (i%1000 == 0)
                {
                    context.SaveChanges();
                }
            }

            context.SaveChanges();

            return result;
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}