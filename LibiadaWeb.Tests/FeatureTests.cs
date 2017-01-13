namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// The feature tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Feature))]
    public class FeatureTests
    {
        /// <summary>
        /// The features count.
        /// </summary>
        private const int FeaturesCount = 28;

        /// <summary>
        /// Tests count of features.
        /// </summary>
        [Test]
        public void FeatureCountTest()
        {
            var actualCount = ArrayExtensions.ToArray<Feature>().Length;
            Assert.AreEqual(FeaturesCount, actualCount);
        }

        /// <summary>
        /// Tests values of features.
        /// </summary>
        [Test]
        public void FeatureValuesTest()
        {
            var features = ArrayExtensions.ToArray<Feature>();

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
        public void FeatureHasDisplayValueTest([Values]Feature feature)
        {
            Assert.IsFalse(string.IsNullOrEmpty(feature.GetDisplayValue()));
        }

        /// <summary>
        /// Tests that all features have description.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        [Test]
        public void FeatureHasDescriptionTest([Values]Feature feature)
        {
            Assert.IsFalse(string.IsNullOrEmpty(feature.GetDescription()));
        }

        /// <summary>
        /// Tests that all features have valid nature attribute.
        /// </summary>
        /// <param name="feature">
        /// The feature.
        /// </param>
        [Test]
        public void FeatureHasNatureTest([Values]Feature feature)
        {
            var natures = ArrayExtensions.ToArray<Nature>();
            Assert.True(natures.Contains(feature.GetNature()));
        }

        /// <summary>
        /// Tests that all features values are unique.
        /// </summary>
        [Test]
        public void FeatureValuesUniqueTest()
        {
            var features = ArrayExtensions.ToArray<Feature>();
            var featureValues = features.Cast<byte>();
            Assert.That(featureValues, Is.Unique);
        }
    }
}
