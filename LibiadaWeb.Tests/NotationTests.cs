namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// Notation enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Notation))]
    public class NotationTests
    {
        /// <summary>
        /// The notations count.
        /// </summary>
        private const int NotationsCount = 10;

        /// <summary>
        /// Tests count of notations.
        /// </summary>
        [Test]
        public void NotationCountTest()
        {
            var actualCount = ArrayExtensions.ToArray<Notation>().Length;
            Assert.AreEqual(NotationsCount, actualCount);
        }

        /// <summary>
        /// Tests values of notations.
        /// </summary>
        [Test]
        public void NotationValuesTest()
        {
            var notations = ArrayExtensions.ToArray<Notation>();

            for (int i = 1; i <= NotationsCount; i++)
            {
                Assert.IsTrue(notations.Contains((Notation)i));
            }
        }

        /// <summary>
        /// Tests names of notations.
        /// </summary>
        /// <param name="notation">
        /// The notation.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((Notation)1, "Nucleotides")]
        [TestCase((Notation)2, "Triplets")]
        [TestCase((Notation)3, "AminoAcids")]
        [TestCase((Notation)4, "GeneticWords")]
        [TestCase((Notation)5, "NormalizedWords")]
        [TestCase((Notation)6, "FormalMotifs")]
        [TestCase((Notation)7, "Measures")]
        [TestCase((Notation)8, "Notes")]
        [TestCase((Notation)9, "Letters")]
        [TestCase((Notation)10, "IntegerValues")]
        public void NotationNamesTest(Notation notation, string name)
        {
            Assert.AreEqual(name, notation.GetName());
        }

        /// <summary>
        /// Tests that all notations have display value.
        /// </summary>
        /// <param name="notation">
        /// The notation.
        /// </param>
        [Test]
        public void NotationHasDisplayValueTest([Values]Notation notation)
        {
            Assert.IsFalse(string.IsNullOrEmpty(notation.GetDisplayValue()));
        }

        /// <summary>
        /// Tests that all notations have description.
        /// </summary>
        /// <param name="notation">
        /// The notation.
        /// </param>
        [Test]
        public void NotationHasDescriptionTest([Values]Notation notation)
        {
            Assert.IsFalse(string.IsNullOrEmpty(notation.GetDescription()));
        }

        /// <summary>
        /// Tests that all notations have valid nature attribute.
        /// </summary>
        /// <param name="notation">
        /// The notation.
        /// </param>
        [Test]
        public void NotationHasNatureTest([Values]Notation notation)
        {
            var natures = ArrayExtensions.ToArray<Nature>();
            Assert.True(natures.Contains(notation.GetNature()));
        }

        /// <summary>
        /// Tests that all notations values are unique.
        /// </summary>
        [Test]
        public void NotationValuesUniqueTest()
        {
            var notations = ArrayExtensions.ToArray<Notation>();
            var notationValues = notations.Cast<byte>();
            Assert.That(notationValues, Is.Unique);
        }
    }
}
