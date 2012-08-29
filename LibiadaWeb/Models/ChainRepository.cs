using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;
using LibiadaWeb;

namespace LibiadaWeb.Models
{ 
    public class ChainRepository : IChainRepository
    {
        LibiadaWebEntities context = new LibiadaWebEntities();

        public IQueryable<chain> All
        {
            get { return context.chain; }
        }

        public IQueryable<chain> AllIncluding(params Expression<Func<chain, object>>[] includeProperties)
        {
            IQueryable<chain> query = context.chain;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public chain Find(long id)
        {
            return context.chain.Single(x => x.id == id);
        }

        public void InsertOrUpdate(chain chain)
        {
            if (chain.id == default(long)) {
                // New entity
                context.chain.AddObject(chain);
            } else {
                // Existing entity
                context.chain.Attach(chain);
                context.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var chain = context.chain.Single(x => x.id == id);
            context.chain.DeleteObject(chain);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> chains)
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
            var allChains = context.chain.Include("matter");
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

        public BaseChain FromDbChainToLibiadaBaseChain(long chainId)
        {
            chain dbChain = context.chain.Single(c => c.id == chainId);
            return FromDbChainToLibiadaBaseChain(dbChain);
        }

        public BaseChain FromDbChainToLibiadaBaseChain(chain dbChain)
        {
            Alphabet alphabet = FromDbAlphabetToLibiadaAlphabet(dbChain);

            int[] building = FromDbBuildingToLibiadaBuilding(dbChain);

            return new BaseChain(building, alphabet);
        }

        public Chain FromDbChainToLibiadaChain(long chainId)
        {
            chain dbChain = context.chain.Single(c => c.id == chainId);
            return FromDbChainToLibiadaChain(dbChain);
        }

        //TODO: вытаскивать сразу и имеющиеся характеристики цепочки
        public Chain FromDbChainToLibiadaChain(chain dbChain)
        {
            Alphabet alphabet = FromDbAlphabetToLibiadaAlphabet(dbChain);

            int[] building = FromDbBuildingToLibiadaBuilding(dbChain);

            return new Chain(building, alphabet);
        }

        public chain FromLibiadaBaseChainToDbChain(BaseChain libiadaChain, int notationId, matter parent)
        {
            chain result;

            bool continueImport = context.matter.Any(m => m.name == parent.name);
            if (!continueImport)
            {
                result = new chain();
                result.dissimilar = false;
                result.building_type_id = 1;
                result.notation_id = notationId;
                result.creation_date = new DateTimeOffset(DateTime.Now);

                FromLibiadaAlphabetToDbAlphabet(libiadaChain.Alphabet, result, notationId);

                parent.chain.Add(result);//TODO: проверить, возможно одно из действий лишнее
                context.chain.AddObject(result);

                context.SaveChanges();
            }
            else
            {
                long matterId = context.matter.Single(m => m.name == parent.name).id;
                result = context.chain.Single(c => c.matter_id == matterId);
            }

            int[] libiadaBuilding = libiadaChain.Building;

            FromLibiadaBuildingToDbBuilding(result, libiadaBuilding);

            context.SaveChanges();

            return result;
        }

        //TODO: сделать в таблице алфавита id чтобы можно было создать репозиторий алфавита и переместить туда методы алфавита
        public Alphabet FromDbAlphabetToLibiadaAlphabet(chain dbChain)
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

        public IEnumerable<alphabet> FromLibiadaAlphabetToDbAlphabet(Alphabet libiadaAlphabet, chain parent, int notationId)
        {
            List<alphabet> dbAlphabet = new List<alphabet>();
            for (int j = 0; j < libiadaAlphabet.Power; j++)
            {
                dbAlphabet.Add(new alphabet());
                dbAlphabet[j].number = j + 1;
                String strElem = libiadaAlphabet[j].ToString();
                if (!context.element.Any(e => e.notation_id == notationId && e.value.Equals(strElem)))
                {
                    throw new Exception("Элемент " + strElem + " не найден в БД.");
                }
                dbAlphabet[j].element = context.element.Single(e => e.notation_id == notationId && e.value.Equals(strElem));

                parent.alphabet.Add(dbAlphabet[j]);//TODO: проверить, возможно одно из действий лишнее
                context.alphabet.AddObject(dbAlphabet[j]);
            }
            context.SaveChanges();

            return dbAlphabet;
        }

        //TODO: создать репозиторий строя и перенести туда методы строя
        public int[] FromDbBuildingToLibiadaBuilding(chain dbChain)
        {
            IEnumerable<building> dbBuilding = dbChain.building.OrderBy(b => b.index);
            return dbBuilding.Select(b => b.number).ToArray();
        }

        public IEnumerable<building> FromLibiadaBuildingToDbBuilding(chain parent, int[] libiadaBuilding)
        {
            List<building> result = context.building.Where(b => b.chain == parent).OrderBy(b => b.index).ToList();
            int createdCount = result.Count;
            for (int i = createdCount; i < libiadaBuilding.Length; i++)
            {
                result.Add(new building());
                result[i].index = i;
                result[i].number = libiadaBuilding[i];

                parent.building.Add(result[i]);//TODO: проверить, возможно одно из действий лишнее
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
 
        public dna_information CreateDnaInformation(string fastaHeader, matter parent)
        {
            dna_information dnaInformation = new dna_information();
            dnaInformation.fasta_header = fastaHeader;

            parent.dna_information = dnaInformation;//TODO: проверить, возможно одно из действий лишнее
            context.dna_information.AddObject(dnaInformation);
            context.SaveChanges();

            return dnaInformation;
        }

        public void Dispose() 
        {
            context.Dispose();
        }
    }
}