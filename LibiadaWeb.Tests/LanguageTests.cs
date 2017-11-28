namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// Language enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Language))]
    public class LanguageTests
    {
        /// <summary>
        /// The languages count.
        /// </summary>
        private const int LanguagesCount = 3;

        /// <summary>
        /// Tests count of languages.
        /// </summary>
        [Test]
        public void LanguageCountTest()
        {
            var actualCount = EnumExtensions.ToArray<Language>().Length;
            Assert.AreEqual(LanguagesCount, actualCount);
        }

        /// <summary>
        /// Tests values of languages.
        /// </summary>
        [Test]
        public void LanguageValuesTest()
        {
            var languages = EnumExtensions.ToArray<Language>();
            for (int i = 1; i <= LanguagesCount; i++)
            {
                Assert.IsTrue(languages.Contains((Language)i));
            }
        }

        /// <summary>
        /// Tests names of languages.
        /// </summary>
        /// <param name="language">
        /// The language.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((Language)1, "Russian")]
        [TestCase((Language)2, "English")]
        [TestCase((Language)3, "German")]
        public void LanguageNamesTest(Language language, string name)
        {
            Assert.AreEqual(name, language.GetName());
        }

        /// <summary>
        /// Tests that all languages have display value.
        /// </summary>
        /// <param name="language">
        /// The language.
        /// </param>
        [Test]
        public void LanguageHasDisplayValueTest([Values]Language language)
        {
            Assert.IsFalse(string.IsNullOrEmpty(language.GetDisplayValue()));
        }

        /// <summary>
        /// Tests that all languages have description.
        /// </summary>
        /// <param name="language">
        /// The language.
        /// </param>
        [Test]
        public void LanguageHasDescriptionTest([Values]Language language)
        {
            Assert.IsFalse(string.IsNullOrEmpty(language.GetDescription()));
        }

        /// <summary>
        /// Tests that all languages values are unique.
        /// </summary>
        [Test]
        public void LanguageValuesUniqueTest()
        {
            var languages = EnumExtensions.ToArray<Language>();
            var languageValues = languages.Cast<byte>();
            Assert.That(languageValues, Is.Unique);
        }
    }
}
