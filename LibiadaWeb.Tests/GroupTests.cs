namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaWeb.Extensions;

    using NUnit.Framework;

    [TestFixture(TestOf = typeof(Group))]
    public class GroupTests
    {
        private const int GroupsCount = 6;

        /// <summary>
        /// Tests count of groups.
        /// </summary>
        [Test]
        public void GroupCountTest()
        {
            var actualCount = EnumExtensions.ToArray<Group>().Length;
            Assert.AreEqual(GroupsCount, actualCount);
        }

        /// <summary>
        /// Tests values of groups.
        /// </summary>
        [Test]
        public void GroupValuesTest()
        {
            var groups = EnumExtensions.ToArray<Group>();

            for (int i = 1; i <= GroupsCount; i++)
            {
                Assert.IsTrue(groups.Contains((Group)i));
            }
        }

        /// <summary>
        /// Tests names of groups.
        /// </summary>
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
        [Test]
        public void GroupHasDescriptionTest([Values]Group group)
        {
            Assert.IsFalse(string.IsNullOrEmpty(group.GetDescription()));
        }

        /// <summary>
        /// Tests that all groups have valid nature attribute.
        /// </summary>
        [Test]
        public void GroupHasNatureTest([Values]Group group)
        {
            var natures = EnumExtensions.ToArray<Nature>();
            Assert.True(natures.Contains(group.GetNature()));
        }

        /// <summary>
        /// Tests that all groups values are unique.
        /// </summary>
        [Test]
        public void GroupValuesUniqueTest()
        {
            var groups = EnumExtensions.ToArray<Group>();
            var groupValues = groups.Cast<byte>();
            Assert.That(groupValues, Is.Unique);
        }
    }
}