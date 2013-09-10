using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.TheoryOfSet;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class ChainRepository : IChainRepository
    {
        private readonly LibiadaWebEntities db;
        private readonly AlphabetRepository alphabetRepository;
        private readonly BuildingRepository buildingRepository;

        public ChainRepository(LibiadaWebEntities db)
        {
            this.db = db;
            alphabetRepository = new AlphabetRepository(db);
            buildingRepository = new BuildingRepository(db);
        }

        public IQueryable<chain> All
        {
            get { return db.chain; }
        }

        public IQueryable<chain> AllIncluding(params Expression<Func<chain, object>>[] includeProperties)
        {
            IQueryable<chain> query = db.chain;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public chain Find(long id)
        {
            return db.chain.Single(x => x.id == id);
        }

        public void InsertOrUpdate(chain chain)
        {
            if (chain.id == default(long))
            {
                // New entity
                db.chain.AddObject(chain);
            }
            else
            {
                // Existing entity
                db.chain.Attach(chain);
                db.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var chain = Find(id);
            db.chain.DeleteObject(chain);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<chain> chains)
        {
            HashSet<long> chainIds = chains != null
                                         ? new HashSet<long>(chains.Select(c => c.id))
                                         : new HashSet<long>();
            var allChains = db.chain.Include("matter");
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
            Alphabet alphabet = alphabetRepository.ToLibiadaAlphabet(chainId);
            int[] building = buildingRepository.ToArray(chainId);
            return new BaseChain(building, alphabet);
        }

        public Chain FromDbChainToLibiadaChain(long chainId)
        {
            Alphabet alphabet = alphabetRepository.ToLibiadaAlphabet(chainId);
            int[] building = buildingRepository.ToArray(chainId);
            return new Chain(building, alphabet);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}