namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb;

    using NUnit.Framework;

    /// <summary>
    /// Nature enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Nature))]
    public class NatureTests
    {
        /// <summary>
        /// The natures count.
        /// </summary>
        private const int NaturesCount = 4;

        /// <summary>
        /// Tests count of natures.
        /// </summary>
        [Test]
        public void NatureCountTest()
        {
            var actualCount = ArrayExtensions.ToArray<Nature>().Length;
            Assert.AreEqual(NaturesCount, actualCount);
        }

        /// <summary>
        /// Tests values of natures.
        /// </summary>
        [Test]
        public void NatureValuesTest()
        {
            var natures = ArrayExtensions.ToArray<Nature>();
            for (int i = 1; i <= NaturesCount; i++)
            {
                Assert.IsTrue(natures.Contains((Nature)i));
            }
        }

        /// <summary>
        /// Tests names of natures.
        /// </summary>
        /// <param name="nature">
        /// The nature.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((Nature)1, "Genetic")]
        [TestCase((Nature)2, "Music")]
        [TestCase((Nature)3, "Literature")]
        [TestCase((Nature)4, "MeasurementData")]
        public void NatureNamesTest(Nature nature, string name)
        {
            Assert.AreEqual(name, nature.GetName());
        }

        /// <summary>
        /// Tests that all natures have display value.
        /// </summary>
        /// <param name="nature">
        /// The nature.
        /// </param>
        [Test]
        public void NatureHasDisplayValueTest([Values]Nature nature)
        {
            Assert.IsFalse(string.IsNullOrEmpty(nature.GetDisplayValue()));
        }

        /// <summary>
        /// Tests that all natures have description.
        /// </summary>
        /// <param name="nature">
        /// The nature.
        /// </param>
        [Test]
        public void NatureHasDescriptionTest([Values]Nature nature)
        {
            Assert.IsFalse(string.IsNullOrEmpty(nature.GetDescription()));
        }

        /// <summary>
        /// Tests that all natures values are unique.
        /// </summary>
        [Test]
        public void NatureValuesUniqueTest()
        {
            var natures = ArrayExtensions.ToArray<Nature>();
            var natureValues = natures.Cast<byte>();
            Assert.That(natureValues, Is.Unique);
        }
    }
}
