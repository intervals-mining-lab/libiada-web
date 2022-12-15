namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// Attribute enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Attribute))]
    public class AttributeTests
    {
        /// <summary>
        /// The attributes count.
        /// </summary>
        private const int AttributesCount = 48;

        /// <summary>
        /// Array of all attributes.
        /// </summary>
        private readonly Attribute[] attributes = EnumExtensions.ToArray<Attribute>();

        /// <summary>
        /// Tests count of attributes.
        /// </summary>
        [Test]
        public void AttributesCountTest() => Assert.AreEqual(AttributesCount, attributes.Length);

        /// <summary>
        /// Tests values of attributes.
        /// </summary>
        [Test]
        public void AttributeValuesTest()
        {
            for (int i = 1; i <= AttributesCount; i++)
            {
                Assert.IsTrue(attributes.Contains((Attribute)i));
            }
        }

        /// <summary>
        /// Tests names of attributes.
        /// </summary>
        /// <param name="attribute">
        /// The attribute.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((Attribute)1, "db_xref")]
        [TestCase((Attribute)2, "protein_id")]
        [TestCase((Attribute)3, "complement")]
        [TestCase((Attribute)4, "complement_join")]
        [TestCase((Attribute)5, "product")]
        [TestCase((Attribute)6, "note")]
        [TestCase((Attribute)7, "codon_start")]
        [TestCase((Attribute)8, "transl_table")]
        [TestCase((Attribute)9, "inference")]
        [TestCase((Attribute)10, "rpt_type")]
        [TestCase((Attribute)11, "locus_tag")]
        [TestCase((Attribute)12, "old_locus_tag")]
        [TestCase((Attribute)13, "gene")]
        [TestCase((Attribute)14, "anticodon")]
        [TestCase((Attribute)15, "EC_number")]
        [TestCase((Attribute)16, "exception")]
        [TestCase((Attribute)17, "gene_synonym")]
        [TestCase((Attribute)18, "pseudo")]
        [TestCase((Attribute)19, "ncRNA_class")]
        [TestCase((Attribute)20, "standard_name")]
        [TestCase((Attribute)21, "rpt_family")]
        [TestCase((Attribute)22, "direction")]
        [TestCase((Attribute)23, "ribosomal_slippage")]
        [TestCase((Attribute)24, "partial")]
        [TestCase((Attribute)25, "codon_recognized")]
        [TestCase((Attribute)26, "bound_moiety")]
        [TestCase((Attribute)27, "rpt_unit_range")]
        [TestCase((Attribute)28, "rpt_unit_seq")]
        [TestCase((Attribute)29, "function")]
        [TestCase((Attribute)30, "transl_except")]
        [TestCase((Attribute)31, "pseudogene")]
        [TestCase((Attribute)32, "mobile_element_type")]
        [TestCase((Attribute)33, "experiment")]
        [TestCase((Attribute)34, "citation")]
        [TestCase((Attribute)35, "regulatory_class")]
        [TestCase((Attribute)36, "artificial_location")]
        [TestCase((Attribute)37, "proviral")]
        [TestCase((Attribute)38, "operon")]
        [TestCase((Attribute)39, "number")]
        [TestCase((Attribute)40, "replace")]
        [TestCase((Attribute)41, "compare")]
        [TestCase((Attribute)42, "allele")]
        [TestCase((Attribute)43, "trans_splicing")]
        [TestCase((Attribute)44, "frequency")]
        [TestCase((Attribute)45, "GO_function")]
        [TestCase((Attribute)46, "GO_component")]
        [TestCase((Attribute)47, "GO_process")]
        [TestCase((Attribute)48, "satellite")]
        public void AttributesDisplayValuesTest(Attribute attribute, string name) => Assert.AreEqual(name, attribute.GetDisplayValue());

        /// <summary>
        /// Tests that all attributes values are unique.
        /// </summary>
        [Test]
        public void AttributeValuesUniqueTest() => Assert.That(attributes.Cast<byte>(), Is.Unique);
    }
}
