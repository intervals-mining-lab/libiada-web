using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics;
using LibiadaCore.Core.Characteristics.Calculators;
using LibiadaCore.Misc.Iterators;

using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories.Catalogs;
using Newtonsoft.Json;

namespace LibiadaWeb.Controllers.Calculators
{
    public class LocalCaluculationWebApiController : ApiController
    {
        public string GetSubsequenceCharacteristic(long subsequenceId, int characteristicTypeLinkId, int windowSize, int step)
        {
            Chain chain;
            IFullCalculator calculator;
            Link link;

            using (var context = new LibiadaWebEntities())
            {
                var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(context);

                string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                calculator = CalculatorsFactory.CreateFullCalculator(className);
                link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                var subsequenceExtractor = new SubsequenceExtractor(context);

                var dbSubsequence = context.Subsequence.Single(s => s.Id == subsequenceId);

                chain = subsequenceExtractor.ExtractChains(new [] {dbSubsequence}).Single();
            }

            CutRule cutRule = new SimpleCutRule(chain.GetLength(), step, windowSize);

            CutRuleIterator iter = cutRule.GetIterator();

            List<Chain> fragments = new List<Chain>();

            while (iter.Next())
            {
                var fragment = new Chain(iter.GetEndPosition() - iter.GetStartPosition());

                for (int k = 0; iter.GetStartPosition() + k < iter.GetEndPosition(); k++)
                {
                    fragment.Set(chain[iter.GetStartPosition() + k], k);
                }

                fragments.Add(fragment);
            }

            var characteristics = new double[fragments.Count];

            for (int k = 0; k < fragments.Count; k++)
            {
                characteristics[k] = calculator.Calculate(fragments[k], link);
            }

            return JsonConvert.SerializeObject(characteristics);
        }
    }
}
