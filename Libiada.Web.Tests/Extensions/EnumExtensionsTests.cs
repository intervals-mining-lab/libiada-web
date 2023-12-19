namespace Libiada.Web.Tests.Extensions
{
    using Libiada.Web.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// Tests for enum extensions.
    /// </summary>
    [TestFixture(TestOf = typeof(EnumExtensions))]
    public class EnumExtensionsTests
    {
        /// <summary>
        /// The test enum.
        /// </summary>
        private enum TestEnum : byte
        {
            /// <summary>
            /// The first.
            /// </summary>
            First = 1,

            /// <summary>
            /// The second.
            /// </summary>
            Second = 2,

            /// <summary>
            /// The third.
            /// </summary>
            Third = 3
        }
    }
}
