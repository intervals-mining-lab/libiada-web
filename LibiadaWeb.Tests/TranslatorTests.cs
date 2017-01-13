namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// Translator enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Translator))]
    public class TranslatorTests
    {
        /// <summary>
        /// The translators count.
        /// </summary>
        private const int TranslatorsCount = 4;

        /// <summary>
        /// Tests count of translators.
        /// </summary>
        [Test]
        public void TranslatorCountTest()
        {
            var actualCount = ArrayExtensions.ToArray<Translator>().Length;
            Assert.AreEqual(TranslatorsCount, actualCount);
        }

        /// <summary>
        /// Tests values of translators.
        /// </summary>
        [Test]
        public void TranslatorValuesTest()
        {
            var translators = ArrayExtensions.ToArray<Translator>();
            for (int i = 0; i < TranslatorsCount; i++)
            {
                Assert.IsTrue(translators.Contains((Translator)i));
            }
        }

        /// <summary>
        /// Tests names of translators.
        /// </summary>
        /// <param name="translator">
        /// The translator.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((Translator)0, "NoneOrManual")]
        [TestCase((Translator)1, "GoogleTranslate")]
        [TestCase((Translator)2, "Promt")]
        [TestCase((Translator)3, "InterTran")]
        public void TranslatorNamesTest(Translator translator, string name)
        {
            Assert.AreEqual(name, translator.GetName());
        }

        /// <summary>
        /// Tests that all translators have display value.
        /// </summary>
        /// <param name="translator">
        /// The translator.
        /// </param>
        [Test]
        public void TranslatorHasDisplayValueTest([Values]Translator translator)
        {
            Assert.IsFalse(string.IsNullOrEmpty(translator.GetDisplayValue()));
        }

        /// <summary>
        /// Tests that all translators have description.
        /// </summary>
        /// <param name="translator">
        /// The translator.
        /// </param>
        [Test]
        public void TranslatorHasDescriptionTest([Values]Translator translator)
        {
            Assert.IsFalse(string.IsNullOrEmpty(translator.GetDescription()));
        }

        /// <summary>
        /// Tests that all translators values are unique.
        /// </summary>
        [Test]
        public void TranslatorValuesUniqueTest()
        {
            var translators = ArrayExtensions.ToArray<Translator>();
            var translatorValues = translators.Cast<byte>();
            Assert.That(translatorValues, Is.Unique);
        }
    }
}
