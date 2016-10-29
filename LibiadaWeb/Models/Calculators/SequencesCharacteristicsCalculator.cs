﻿namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaCore.Misc;

    using LibiadaWeb.Models.Repositories.Calculators;

    public static class SequencesCharacteristicsCalculator
    {
        public static double[][] Calculate(Chain[][] chains, IFullCalculator[] calculators, Link[] links, int[] characteristicTypeLinkIds, LibiadaWebEntities db)
        {
            var newCharacteristics = new List<Characteristic>();
            var characteristicRepository = new CharacteristicRepository(db);
            var characteristics = new double[chains.Length][];

            var sequenceIds = chains.SelectMany(c => c).Select(c => c.Id).Distinct();

            var dbCharacteristics = db.Characteristic
                                              .Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && sequenceIds.Contains(c.SequenceId))
                                              .ToArray()
                                              .GroupBy(c => c.SequenceId)
                                              .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

            for (int i = 0; i < chains.Length; i++)
            {
                characteristics[i] = new double[calculators.Length];

                for (int j = 0; j < calculators.Length; j++)
                {

                    long sequenceId = chains[i][j].Id;
                    chains[i][j].FillIntervalManagers();

                    Dictionary<int, double> sequenceDbCharacteristics;
                    if (!dbCharacteristics.TryGetValue(sequenceId, out sequenceDbCharacteristics))
                    {
                        sequenceDbCharacteristics = new Dictionary<int, double>();
                    }

                    if (!sequenceDbCharacteristics.TryGetValue(characteristicTypeLinkIds[j], out characteristics[i][j]))
                    {
                        characteristics[i][j] = calculators[j].Calculate(chains[i][j], links[j]);
                        var currentCharacteristic = new Characteristic
                        {
                            SequenceId = sequenceId,
                            CharacteristicTypeLinkId = characteristicTypeLinkIds[j],
                            Value = characteristics[i][j]
                        };

                        newCharacteristics.Add(currentCharacteristic);
                    }
                }
            }

            // trying to save calculated characteristics to database
            characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

            return characteristics;
        }

        public static double[][] Calculate(Chain[][] chains, IFullCalculator[] calculators, Link[] links, bool rotate, bool complementary, uint? rotationLength)
        {
            var characteristics = new double[chains.Length][];

            for (int i = 0; i < chains.Length; i++)
            {
                characteristics[i] = new double[calculators.Length];

                for (int j = 0; j < calculators.Length; j++)
                {
                    if (complementary)
                    {
                        var sourceSequence = new Sequence(Alphabets.DNA, chains[i][j].ToString());
                        var complementarySequence = sourceSequence.GetReverseComplementedSequence();
                        chains[i][j] = new Chain(complementarySequence.ConvertToString());
                    }

                    if (rotate)
                    {
                        var building = chains[i][j].Building.Rotate(rotationLength ?? 0);
                        var newSequence = building.Select(t => new ValueInt(t)).Cast<IBaseObject>().ToList();
                        chains[i][j] = new Chain(newSequence);
                    }

                    chains[i][j].FillIntervalManagers();

                    characteristics[i][j] = calculators[j].Calculate(chains[i][j], links[j]);
                }
            }

            return characteristics;
        }
    }
}