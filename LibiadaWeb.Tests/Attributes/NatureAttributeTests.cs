namespace LibiadaWeb.Tests.Attributes
{
    using System;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Attributes;

    using NUnit.Framework;

    /// <summary>
    /// The nature attribute tests.
    /// </summary>
    [TestFixture(TestOf = typeof(NatureAttribute))]
    public class NatureAttributeTests
    {
        /// <summary>
        /// Invalid nature value test.
        /// </summary>
        [Test]
        public void InvalidNatureValueTest()
        {
            Assert.Throws<ArgumentException>(() => new NatureAttribute((Nature)10));
            Assert.Throws<ArgumentException>(() => new NatureAttribute((Nature)0));
        }

        /// <summary>
        /// Nature attribute value test.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        [Test]
        public void NatureAttributeValueTest([Values] Nature value)
        {
            var attribute = new NatureAttribute(value);
            Assert.AreEqual(value, attribute.Value);
            Assert.AreEqual(value.GetDisplayValue(), attribute.Value.GetDisplayValue());
        }
    }
}
