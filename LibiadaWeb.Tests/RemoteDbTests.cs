﻿namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;

    using NUnit.Framework;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// RemoteDb enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(RemoteDb))]
    public class RemoteDbTests
    {
        /// <summary>
        /// The remote dbs count.
        /// </summary>
        private const int RemoteDbsCount = 1;

        /// <summary>
        /// Array of all remote databases.
        /// </summary>
        private readonly RemoteDb[] remoteDbs = EnumExtensions.ToArray<RemoteDb>();

        /// <summary>
        /// Array of all natures.
        /// </summary>
        private readonly Nature[] natures = EnumExtensions.ToArray<Nature>();

        /// <summary>
        /// Tests count of remote dbs.
        /// </summary>
        [Test]
        public void RemoteDbCountTest() => Assert.AreEqual(RemoteDbsCount, remoteDbs.Length);

        /// <summary>
        /// Tests values of remote dbs.
        /// </summary>
        [Test]
        public void RemoteDbValuesTest()
        {
            for (int i = 1; i <= RemoteDbsCount; i++)
            {
                Assert.IsTrue(remoteDbs.Contains((RemoteDb)i));
            }
        }

        /// <summary>
        /// Tests names of remote dbs.
        /// </summary>
        /// <param name="remoteDb">
        /// The remote db.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((RemoteDb)1, "GenBank")]
        public void RemoteDbNamesTest(RemoteDb remoteDb, string name) => Assert.AreEqual(name, remoteDb.GetName());

        /// <summary>
        /// Tests that all remote dbs have display value.
        /// </summary>
        /// <param name="remoteDb">
        /// The remote db.
        /// </param>
        [Test]
        public void RemoteDbHasDisplayValueTest([Values]RemoteDb remoteDb) => Assert.That(remoteDb.GetDisplayValue(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all remote dbs have description.
        /// </summary>
        /// <param name="remoteDb">
        /// The remote db.
        /// </param>
        [Test]
        public void RemoteDbHasDescriptionTest([Values]RemoteDb remoteDb) => Assert.That(remoteDb.GetDescription(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all remote dbs have valid nature attribute.
        /// </summary>
        /// <param name="remoteDb">
        /// The remote db.
        /// </param>
        [Test]
        public void RemoteDbHasNatureTest([Values]RemoteDb remoteDb) => Assert.True(natures.Contains(remoteDb.GetNature()));

        /// <summary>
        /// Tests that all remote dbs values are unique.
        /// </summary>
        [Test]
        public void RemoteDbValuesUniqueTest() => Assert.That(remoteDbs.Cast<byte>(), Is.Unique);
    }
}
