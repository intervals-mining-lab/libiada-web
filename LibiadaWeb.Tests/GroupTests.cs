namespace LibiadaWeb.Tests
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;

    using NUnit.Framework;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// Group enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(Group))]
    public class GroupTests
    {
        /// <summary>
        /// The groups count.
        /// </summary>
        private const int GroupsCount = 9;

        /// <summary>
        /// Array of all groups.
        /// </summary>
        private readonly Group[] groups = EnumExtensions.ToArray<Group>();

        /// <summary>
        /// Array of all natures.
        /// </summary>
        private readonly Nature[] natures = EnumExtensions.ToArray<Nature>();

        /// <summary>
        /// Tests count of groups.
        /// </summary>
        [Test]
        public void GroupCountTest() => Assert.AreEqual(GroupsCount, groups.Length);

        /// <summary>
        /// Tests values of groups.
        /// </summary>
        [Test]
        public void GroupValuesTest()
        {
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
        [TestCase((Group)7, "Painting")]
        [TestCase((Group)8, "Photo")]
        [TestCase((Group)9, "Picture")]
        public void GroupNamesTest(Group group, string name) => Assert.AreEqual(name, @group.GetName());

        /// <summary>
        /// Tests that all groups have display value.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        [Test]
        public void GroupHasDisplayValueTest([Values]Group group) => Assert.That(group.GetDisplayValue(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all groups have description.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        [Test]
        public void GroupHasDescriptionTest([Values]Group group) => Assert.That(group.GetDescription(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all groups have valid nature attribute.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        [Test]
        public void GroupHasNatureTest([Values]Group group) => Assert.True(natures.Contains(@group.GetNature()));

        /// <summary>
        /// Tests that all groups values are unique.
        /// </summary>
        [Test]
        public void GroupValuesUniqueTest() => Assert.That(groups.Cast<byte>(), Is.Unique);
    }
}
