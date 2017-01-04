namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaWeb;
    using LibiadaWeb.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// The nature enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Nature))]
    public class NatureTests
    {
        private const int NaturesCount = 4;

        /// <summary>
        /// Tests count of natures.
        /// </summary>
        [Test]
        public void NatureCountTest()
        {
            var actualCount = EnumExtensions.ToArray<Nature>().Length;
            Assert.AreEqual(NaturesCount, actualCount);
        }

        /// <summary>
        /// Tests values of natures.
        /// </summary>
        [Test]
        public void NatureValuesTest()
        {
            var natures = EnumExtensions.ToArray<Nature>();
            for (int i = 1; i <= NaturesCount; i++)
            {
                Assert.IsTrue(natures.Contains((Nature)i));
            }
        }

        /// <summary>
        /// Tests names of natures.
        /// </summary>
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
            var natures = EnumExtensions.ToArray<Nature>();
            var natureValues = natures.Cast<byte>();
            Assert.That(natureValues, Is.Unique);
        }
    }
}
