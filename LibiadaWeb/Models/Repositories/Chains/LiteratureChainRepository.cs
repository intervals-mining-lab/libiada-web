using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories.Chains
{
    public class LiteratureChainRepository : ILiteratureChainRepository
    {
        private readonly LibiadaWebEntities db;

        public LiteratureChainRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<literature_chain> All
        {
            get { return db.literature_chain; }
        }

        public IQueryable<literature_chain> AllIncluding(
            params Expression<Func<literature_chain, object>>[] includeProperties)
        {
            IQueryable<literature_chain> query = db.literature_chain;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public literature_chain Find(long id)
        {
            return db.literature_chain.Single(x => x.id == id);
        }

        public void InsertOrUpdate(literature_chain literature_chain)
        {
            if (literature_chain.id == default(long))
            {
                // New entity
                db.literature_chain.AddObject(literature_chain);
            }
            else
            {
                // Existing entity
                db.literature_chain.Attach(literature_chain);
                db.ObjectStateManager.ChangeObjectState(literature_chain, EntityState.Modified);
            }
        }

        public void Delete(long id)
        {
            var literature_chain = Find(id);
            db.literature_chain.DeleteObject(literature_chain);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<literature_chain> chains)
        {
            HashSet<long> chainIds = chains != null
                                         ? new HashSet<long>(chains.Select(c => c.id))
                                         : new HashSet<long>();
            var allChains = db.literature_chain.Include("matter");
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

        //TODO: создать репозиторий стро€ и перенести туда методы стро€
        public int[] FromDbBuildingToLibiadaBuilding(literature_chain dbChain)
        {
            String query = "SELECT number FROM building WHERE chain_id = " + dbChain.id + " ORDER BY index";
            return db.ExecuteStoreQuery<int>(query).ToArray();
        }

        public void FromLibiadaBuildingToDbBuilding(literature_chain parent, int[] libiadaBuilding)
        {
            int createdCount = db.ExecuteStoreQuery<int>("SELECT get_building_count('" + parent.id + "')").First();
            for (int i = createdCount; i < libiadaBuilding.Length; i++)
            {
                building elem = new building { index = i, number = libiadaBuilding[i] };

                parent.building.Add(elem);

                //костыль чтобы Ѕƒ реже умирала
                if (i % 1000 == 0)
                {
                    db.SaveChanges();
                }
            }

            db.SaveChanges();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}