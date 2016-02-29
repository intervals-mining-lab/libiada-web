namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaWeb;
    using LibiadaWeb.Helpers;

    using NUnit.Framework;

    /// <summary>
    /// The nature enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Nature))]
    public class NatureTests
    {
        /// <summary>
        /// Tests count of natures.
        /// </summary>
        [Test]
        public void NatureCountTest()
        {
            const int ExpectedCount = 4;
            var actualCount = EnumExtensions.ToArray<Nature>().Length;
            Assert.AreEqual(ExpectedCount, actualCount);
        }

        /// <summary>
        /// Tests values of natures.
        /// </summary>
        [Test]
        public void NatureValuesTest()
        {
            var natures = EnumExtensions.ToArray<Nature>();

            Assert.IsTrue(natures.Contains((Nature)1));
            Assert.IsTrue(natures.Contains((Nature)2));
            Assert.IsTrue(natures.Contains((Nature)3));
            Assert.IsTrue(natures.Contains((Nature)4));
        }

        /// <summary>
        /// Tests names of natures.
        /// </summary>
        [Test]
        public void NatureNamesTest()
        {
            Assert.AreEqual("Genetic", ((Nature)1).GetName());
            Assert.AreEqual("Music", ((Nature)2).GetName());
            Assert.AreEqual("Literature", ((Nature)3).GetName());
            Assert.AreEqual("MeasurementData", ((Nature)4).GetName());
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
        public void NatureHasDescriptionTest()
        {
            var natures = EnumExtensions.ToArray<Nature>();
            foreach (var nature in natures)
            {
                Assert.IsFalse(string.IsNullOrEmpty(nature.GetDescription()));
            }
        }

        /// <summary>
        /// Tests that all nature values are unique.
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
