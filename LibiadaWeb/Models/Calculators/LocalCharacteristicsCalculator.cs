namespace LibiadaWeb.Models.Calculators
{
    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Iterators;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class LocalCharacteristicsCalculator
    {
        /// <summary>
        /// The get subsequence characteristic.
        /// </summary>
        /// <param name="subsequenceId">
        /// The subsequence id.
        /// </param>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="windowSize">
        /// The window size.
        /// </param>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public double[] GetSubsequenceCharacteristic(
            long subsequenceId,
            short characteristicLinkId,
            int windowSize,
            int step)
        {
            Chain chain;
            IFullCalculator calculator;
            Link link;

            using (var db = new LibiadaWebEntities())
            {
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;

                FullCharacteristic characteristic =
                    characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
                link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

                var subsequenceExtractor = new SubsequenceExtractor(db);

                Subsequence subsequence = db.Subsequence.Single(s => s.Id == subsequenceId);
                chain = subsequenceExtractor.GetSubsequenceSequence(subsequence);
            }

            CutRule cutRule = new SimpleCutRule(chain.Length, step, windowSize);

            CutRuleIterator iterator = cutRule.GetIterator();

            var fragments = new List<Chain>();

            while (iterator.Next())
            {
                int start = iterator.GetStartPosition();
                int end = iterator.GetEndPosition();

                var fragment = new List<IBaseObject>();
                for (int k = 0; start + k < end; k++)
                {
                    fragment.Add(chain[start + k]);
                }

                fragments.Add(new Chain(fragment));
            }

            var characteristics = new double[fragments.Count];

            for (int k = 0; k < fragments.Count; k++)
            {
                characteristics[k] = calculator.Calculate(fragments[k], link);
            }

            return characteristics;
        }
    }
}