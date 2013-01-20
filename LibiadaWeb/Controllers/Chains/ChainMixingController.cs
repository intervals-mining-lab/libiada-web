using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    public class ChainMixingController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly MatterRepository matterRepository;
        private readonly NotationRepository notationRepository;
        private readonly ChainRepository chainRepository;
        private readonly AlphabetRepository alphabetRepository;
        private readonly Random rndGenerator = new Random();

        public ChainMixingController()
        {
            matterRepository = new MatterRepository(db);
            notationRepository = new NotationRepository(db);
            chainRepository = new ChainRepository(db);
            alphabetRepository = new AlphabetRepository(db);
        }

        //
        // GET: /ChainMixing/

        public ActionResult Index()
        {

            ViewBag.matters = db.matter.ToList();
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long matterId, int notationId, int languageId, int mixes)
        {
            matter dbMatter = db.matter.Single(m => m.id == matterId);
            chain dbChain;
            if (dbMatter.nature_id == 3)
            {
                long chainId =
                    db.literature_chain.Single(
                        l => l.matter_id == matterId && l.notation_id == notationId && l.language_id == languageId).id;
                dbChain = db.chain.Single(c => c.id == chainId);
            }
            else
            {
                dbChain = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId);
            }
            BaseChain libiadaChain = chainRepository.FromDbChainToLibiadaBaseChain(dbChain.id);
            for (int i = 0; i < mixes; i++)
            {
                int firstIndex = rndGenerator.Next(libiadaChain.Length);
                int secondIndex = rndGenerator.Next(libiadaChain.Length);

                IBaseObject firstElement = libiadaChain[firstIndex];
                IBaseObject secondElement = libiadaChain[secondIndex];
                libiadaChain[firstIndex] = secondElement;
                libiadaChain[secondIndex] = firstElement;
            }
            matter result = new matter
                {
                    nature_id = dbMatter.nature_id,
                    name = dbMatter.name + " " + mixes + " перемешиваний"
                };
            db.matter.AddObject(result);

            chain resultChain = new chain()
            {
                id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                dissimilar = false,
                notation_id = notationId,
                piece_type_id = 1,
                creation_date = new DateTimeOffset(DateTime.Now)
            };

            result.chain.Add(resultChain); //TODO: проверить, возможно одно из действий лишнее
            db.chain.AddObject(resultChain);
            IEnumerable<alphabet> alphabet = alphabetRepository.FromLibiadaAlphabetToDbAlphabet(libiadaChain.Alphabet, notationId, resultChain.id, false);
            chainRepository.FromLibiadaBuildingToDbBuilding(resultChain, libiadaChain.Building);
            db.SaveChanges();
            return RedirectToAction("Index", "Matter");
        }
    }
}
