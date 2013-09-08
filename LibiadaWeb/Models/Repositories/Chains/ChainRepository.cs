using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.TheoryOfSet;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class ChainRepository : IChainRepository
    {
        private readonly LibiadaWebEntities db;
        private readonly AlphabetRepository alphabetRepository;

        public ChainRepository(LibiadaWebEntities db)
        {
            this.db = db;
            alphabetRepository = new AlphabetRepository(db);
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
            chain dbChain = db.chain.Single(c => c.id == chainId);
            return FromDbChainToLibiadaBaseChain(dbChain);
        }

        public BaseChain FromDbChainToLibiadaBaseChain(chain dbChain)
        {
            Alphabet alphabet =
                alphabetRepository.FromDbAlphabetToLibiadaAlphabet(dbChain.alphabet.OrderBy(a => a.number));

            int[] building = FromDbBuildingToLibiadaBuilding(dbChain);

            return new BaseChain(building, alphabet);
        }

        public Chain FromDbChainToLibiadaChain(long chainId)
        {
            chain dbChain = db.chain.Single(c => c.id == chainId);
            return FromDbChainToLibiadaChain(dbChain);
        }

        public Chain FromDbChainToLibiadaChain(chain dbChain)
        {
            Alphabet alphabet =
                alphabetRepository.FromDbAlphabetToLibiadaAlphabet(dbChain.alphabet.OrderBy(a => a.number));

            int[] building = FromDbBuildingToLibiadaBuilding(dbChain);

            return new Chain(building, alphabet);
        }

        public chain FromLibiadaBaseChainToDbChain(BaseChain libiadaChain, int notationId, matter parent)
        {
            chain result;

            bool continueImport = db.matter.Any(m => m.name == parent.name);
            if (!continueImport)
            {
                result = new chain
                    {
                        id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                        dissimilar = false,
                        notation_id = notationId,
                        creation_date = DateTime.Now
                    };

                IEnumerable<alphabet> alphabet =
                    alphabetRepository.FromLibiadaAlphabetToDbAlphabet(libiadaChain.Alphabet, notationId, result.id,
                                                                       false);

                result.alphabet.Attach(alphabet);
                parent.chain.Add(result);

                db.SaveChanges();
            }
            else
            {
                result = db.chain.Single(c => c.matter_id == parent.id);
            }

            int[] libiadaBuilding = libiadaChain.Building;

            FromLibiadaBuildingToDbBuilding(result, libiadaBuilding);

            db.SaveChanges();

            return result;
        }



        //TODO: создать репозиторий строя и перенести туда методы строя
        public int[] FromDbBuildingToLibiadaBuilding(chain dbChain)
        {
            String query = "SELECT number FROM building WHERE chain_id = " + dbChain.id + " ORDER BY index";
            return db.ExecuteStoreQuery<int>(query).ToArray();
        }

        public void FromLibiadaBuildingToDbBuilding(chain parent, int[] libiadaBuilding)
        {
            String aggregatedBuilding = libiadaBuilding.Aggregate(new StringBuilder(), (a, b) =>
                                                                  a.Append(", " + b.ToString()),
                                                                  a => a.Remove(0, 2).ToString());
            String query = "SELECT create_building_from_string(" + parent.id + ", '" + aggregatedBuilding + "')";
            db.ExecuteStoreQuery<String>(query);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}