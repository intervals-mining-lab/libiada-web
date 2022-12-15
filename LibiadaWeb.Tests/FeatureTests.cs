namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;

    using NUnit.Framework;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// Feature enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Feature))]
    public class FeatureTests
    {
        /// <summary>
        /// The features count.
        /// </summary>
        private const int FeaturesCount = 39;

        /// <summary>
        /// Array of all features.
        /// </summary>
        private readonly Feature[] features = EnumExtensions.ToArray<Feature>();

        /// <summary>
        /// Array of all natures.
        /// </summary>
        private readonly Nature[] natures = EnumExtensions.ToArray<Nature>();

        /// <summary>
        /// Tests count of features.
        /// </summary>
        [Test]
        public void FeatureCountTest() => Assert.AreEqual(FeaturesCount, features.Length);

        /// <summary>
        /// Tests values of features.
        /// </summary>
        [Test]
        public void FeatureValuesTest()
        {
            for (int i = 0; i < FeaturesCount; i++)
            {
                Assert.IsTrue(features.Contains((Feature)i));
            }
        }

        /// <summary>
        /// Tests names of features.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="genBankName">
        /// GenBank name.
        /// </param>
        [TestCase((Feature)0, "NonCodingSequence", "")]
        [TestCase((Feature)1, "CodingSequence", "CDS")]
        [TestCase((Feature)2, "RibosomalRNA", "rRNA")]
        [TestCase((Feature)3, "TransferRNA", "tRNA")]
        [TestCase((Feature)4, "NoncodingRNA", "ncRNA")]
        [TestCase((Feature)5, "TransferMessengerRNA", "tmRNA")]
        [TestCase((Feature)6, "PseudoGen", "pseudo")]
        [TestCase((Feature)7, "RepeatRegion", "repeat_region")]
        [TestCase((Feature)8, "MiscellaneousOtherRNA", "misc_RNA")]
        [TestCase((Feature)9, "MiscellaneousFeature", "misc_feature")]
        [TestCase((Feature)10, "MessengerRNA", "mRNA")]
        [TestCase((Feature)11, "Regulatory", "regulatory")]
        [TestCase((Feature)12, "SequenceTaggedSite", "STS")]
        [TestCase((Feature)13, "OriginOfReplication", "rep_origin")]
        [TestCase((Feature)14, "SignalPeptideCodingSequence", "sig_peptide")]
        [TestCase((Feature)15, "MiscellaneousBinding", "misc_binding")]
        [TestCase((Feature)16, "StemLoop", "stem_loop")]
        [TestCase((Feature)17, "DisplacementLoop", "D-loop")]
        [TestCase((Feature)18, "DiversitySegment", "D_segment")]
        [TestCase((Feature)19, "MobileElement", "mobile_element")]
        [TestCase((Feature)20, "Variation", "variation")]
        [TestCase((Feature)21, "ProteinBind", "protein_bind")]
        [TestCase((Feature)22, "MaturePeptid", "mat_peptide")]
        [TestCase((Feature)23, "MiscellaneousDifference", "misc_difference")]
        [TestCase((Feature)24, "Gene", "gene")]
        [TestCase((Feature)25, "ThreeEnd", "3'UTR")]
        [TestCase((Feature)26, "FiveEnd", "5'UTR")]
        [TestCase((Feature)27, "PrimerBind", "primer_bind")]
        [TestCase((Feature)28, "Intron", "intron")]
        [TestCase((Feature)29, "Operon", "operon")]
        [TestCase((Feature)30, "PolyASite", "polyA_site")]
        [TestCase((Feature)31, "ModifiedBase", "modified_base")]
        [TestCase((Feature)32, "MiscellaneousRecombination", "misc_recomb")]
        [TestCase((Feature)33, "Exon", "exon")]
        [TestCase((Feature)34, "Unsure", "unsure")]
        [TestCase((Feature)35, "PrecursorRNA", "precursor_RNA")]
        [TestCase((Feature)36, "Telomere", "telomere")]
        [TestCase((Feature)37, "ConstantRegion", "C_region")]
        [TestCase((Feature)38, "MiscellaneousStructure", "misc_structure")]
        public void FeatureNameAndGenBankNameTest(Feature feature, string name, string genBankName)
        {
            Assert.AreEqual(name, feature.GetName());
            Assert.AreEqual(genBankName, feature.GetGenBankName());
        }

        /// <summary>
        /// Tests that all features have display value.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        [Test]
        public void FeatureHasDisplayValueTest([Values]Feature feature) => Assert.That(feature.GetDisplayValue(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all features have description.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        [Test]
        public void FeatureHasDescriptionTest([Values]Feature feature) => Assert.That(feature.GetDescription(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all features have valid nature attribute.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        [Test]
        public void FeatureHasValidNatureTest([Values]Feature feature) => Assert.True(natures.Contains(feature.GetNature()));

        /// <summary>
        /// Tests that all features values are unique.
        /// </summary>
        [Test]
        public void FeatureValuesUniqueTest() => Assert.That(features.Cast<byte>(), Is.Unique);
    }
}
