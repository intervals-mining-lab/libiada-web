namespace LibiadaWeb.Tests.Models.Repositories.Sequences
{
    using LibiadaWeb.Models.Repositories.Sequences;

    using NUnit.Framework;

    /// <summary>
    /// The matter repository tests.
    /// </summary>
    [TestFixture]
    public class MatterRepositoryTests
    {
        /// <summary>
        /// Determining of group and sequence type from name test.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="nature">
        /// The nature.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="sequenceType">
        /// The sequence type.
        /// </param>
        [TestCase("Rickettsia bellii OSU 85-389", Nature.Genetic, Group.Bacteria, SequenceType.CompleteGenome)]
        [TestCase("Rickettsia australis str. Cutlack Plasmid01", Nature.Genetic, Group.Bacteria, SequenceType.Plasmid)]
        [TestCase("Rickettsia japonica strain YH 16S ribosomal RNA gene, complete sequence", Nature.Genetic, Group.Bacteria, SequenceType.RRNA16S)]
        [TestCase("Feline morbillivirus Viruses; ssRNA viruses; ssRNA negative-strand viruses; | Feline morbillivirus viral cRNA, complete genome, strain: OtJP001.", Nature.Genetic, Group.Virus, SequenceType.CompleteGenome)]
        [TestCase("Cricetulus griseus 18S ribosomal RNA (Rn18s), ribosomal RNA", Nature.Genetic, Group.Eucariote, SequenceType.RRNA18S)]
        [TestCase("Leptotrombidium akamushi mitochondrion, complete genome", Nature.Genetic, Group.Eucariote, SequenceType.MitochondrionGenome)]
        [TestCase("Odontella sinensis chloroplast, complete genome", Nature.Genetic, Group.Eucariote, SequenceType.ChloroplastGenome)]
        [TestCase("Nicotiana tabacum plastid, complete genome", Nature.Genetic, Group.Eucariote, SequenceType.Plastid)]
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
