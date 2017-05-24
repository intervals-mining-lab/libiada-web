using LibiadaWeb.Models.Repositories.Sequences;
using NUnit.Framework;

namespace LibiadaWeb.Tests.Models.Repositories.Sequences
{
    [TestFixture]
    class MatterRepositoryTests
    {
        [TestCase("Rickettsia bellii OSU 85-389", Nature.Genetic, Group.Bacteria, SequenceType.CompleteGenome)]
        [TestCase("Rickettsia australis str. Cutlack Plasmid01", Nature.Genetic, Group.Bacteria, SequenceType.Plasmid)]
        [TestCase("", Nature.Genetic, Group.Bacteria, SequenceType.RRNA16S)]
        [TestCase("Feline morbillivirus Viruses; ssRNA viruses; ssRNA negative-strand viruses; | Feline morbillivirus viral cRNA, complete genome, strain: OtJP001.", Nature.Genetic, Group.Virus, SequenceType.CompleteGenome)]
        [TestCase("rf4hurccjke", Nature.Genetic, Group.Eucariote, SequenceType.RRNA18S)]
        [TestCase("rf4hurccjke", Nature.Genetic, Group.Eucariote, SequenceType.MitochondrionGenome)]
        [TestCase("rf4hurccjke", Nature.Genetic, Group.Eucariote, SequenceType.ChloroplastGenome)]
        [TestCase("rf4hurccjke", Nature.Genetic, Group.Eucariote, SequenceType.Plastid)]
        [TestCase("rf4hurccjke", Nature.Literature, Group.ClassicalLiterature, SequenceType.CompleteText)]
        [TestCase("rf4hurccjke", Nature.Music, Group.ClassicalMusic, SequenceType.CompleteMusicalComposition)]
        [TestCase("rf4hurccjke", Nature.MeasurementData, Group.ObservationData, SequenceType.CompleteNumericSequence)]
        public void FillGroupAndSequenceTypeTest(string name, Nature nature, Group group, SequenceType sequenceType)
        {
            var matter = new Matter
            {
                Name = name,
                Nature = nature
            };

            MatterRepository.FillGroupAndSequenceType(matter);

            Assert.AreEqual(group, matter.Group);
            Assert.AreEqual(sequenceType, matter.SequenceType);
        }
    }
}
