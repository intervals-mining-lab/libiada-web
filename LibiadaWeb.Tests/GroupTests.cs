namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// The group tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Group))]
    public class GroupTests
    {
        /// <summary>
        /// The groups count.
        /// </summary>
        private const int GroupsCount = 6;

        /// <summary>
        /// Tests count of groups.
        /// </summary>
        [Test]
        public void GroupCountTest()
        {
            var actualCount = ArrayExtensions.ToArray<Group>().Length;
            Assert.AreEqual(GroupsCount, actualCount);
        }

        /// <summary>
        /// Tests values of groups.
        /// </summary>
        [Test]
        public void GroupValuesTest()
        {
            var groups = ArrayExtensions.ToArray<Group>();

            for (int i = 1; i <= GroupsCount; i++)
            {
                Assert.IsTrue(groups.Contains((Group)i));
            }
        }

        /// <summary>
        /// Tests names of groups.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((Group)1, "Bacteria")]
        [TestCase((Group)2, "ClassicalMusic")]
        [TestCase((Group)3, "ClassicalLiterature")]
        [TestCase((Group)4, "ObservationData")]
        [TestCase((Group)5, "Virus")]
        [TestCase((Group)6, "Eucariote")]
        public void GroupNamesTest(Group group, string name)
        {
            Assert.AreEqual(name, group.GetName());
        }

        /// <summary>
        /// Tests that all groups have display value.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        [Test]
        public void GroupHasDisplayValueTest([Values]Group group)
        {
            Assert.IsFalse(string.IsNullOrEmpty(group.GetDisplayValue()));
        }

        /// <summary>
        /// Tests that all groups have description.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        [Test]
        public void GroupHasDescriptionTest([Values]Group group)
        {
            Assert.IsFalse(string.IsNullOrEmpty(group.GetDescription()));
        }

        /// <summary>
        /// Tests that all groups have valid nature attribute.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        [Test]
        public void GroupHasNatureTest([Values]Group group)
        {
            var natures = ArrayExtensions.ToArray<Nature>();
            Assert.True(natures.Contains(group.GetNature()));
        }

        /// <summary>
        /// Tests that all groups values are unique.
        /// </summary>
        [Test]
        public void GroupValuesUniqueTest()
        {
            var groups = ArrayExtensions.ToArray<Group>();
            var groupValues = groups.Cast<byte>();
            Assert.That(groupValues, Is.Unique);
        }
    }
}
