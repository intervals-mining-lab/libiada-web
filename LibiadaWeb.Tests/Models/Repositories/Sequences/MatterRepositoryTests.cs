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
        [TestCase("Leptotrombidium akamushi mitochondrion, complete genome", Nature.Genetic, Group.Eucariote, SequenceType.MitochondrialGenome)]
        [TestCase("Odontella sinensis chloroplast, complete genome", Nature.Genetic, Group.Eucariote, SequenceType.ChloroplastGenome)]
        [TestCase("Nicotiana tabacum plastid, complete genome", Nature.Genetic, Group.Eucariote, SequenceType.Plastid)]
        [TestCase("rf4hurccjke", Nature.Literature, Group.ClassicalLiterature, SequenceType.CompleteText)]
        [TestCase("rf4hurccjke", Nature.Music, Group.ClassicalMusic, SequenceType.CompleteMusicalComposition)]
        [TestCase("rf4hurccjke", Nature.MeasurementData, Group.ObservationData, SequenceType.CompleteNumericSequence)]
        [TestCase("rf4hurccjke", Nature.Image, Group.Picture, SequenceType.CompleteImage)]
        public void GetGroupAndSequenceTypeTest(string name, Nature nature, Group expectedGroup, SequenceType expectedSequenceType)
        {

            (Group group, SequenceType sequenceType) = MatterRepository.GetGroupAndSequenceType(name, nature);

            Assert.AreEqual(expectedGroup, group);
            Assert.AreEqual(expectedSequenceType, sequenceType);
        }
    }
}
