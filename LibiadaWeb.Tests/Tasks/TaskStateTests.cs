namespace LibiadaWeb.Tests.Tasks
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Tasks;

    using NUnit.Framework;

    /// <summary>
    /// TaskState enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(TaskState))]
    public class TaskStateTests
    {
        /// <summary>
        /// The task states count.
        /// </summary>
        private const int TaskStatesCount = 4;

        /// <summary>
        /// Array of all task states.
        /// </summary>
        private readonly TaskState[] taskStates = EnumExtensions.ToArray<TaskState>();

        /// <summary>
        /// Tests count of task states.
        /// </summary>
        [Test]
        public void TaskStateCountTest() => Assert.AreEqual(TaskStatesCount, taskStates.Length);

        /// <summary>
        /// Tests values of task states.
        /// </summary>
        [Test]
        public void TaskStateValuesTest()
        {
            for (int i = 1; i <= TaskStatesCount; i++)
            {
                Assert.IsTrue(taskStates.Contains((TaskState)i));
            }
        }

        /// <summary>
        /// Tests names of task states.
        /// </summary>
        /// <param name="taskState">
        /// The task state.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((TaskState)1, "InQueue")]
        [TestCase((TaskState)2, "InProgress")]
        [TestCase((TaskState)3, "Completed")]
        [TestCase((TaskState)4, "Error")]
        public void TaskStateNameTest(TaskState taskState, string name) => Assert.AreEqual(name, taskState.GetName());

        /// <summary>
        /// Tests that all task states have display value.
        /// </summary>
        /// <param name="taskState">
        /// The task state.
        /// </param>
        [Test]
        public void TaskStateHasDisplayValueTest([Values]TaskState taskState) => Assert.That(taskState.GetDisplayValue(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all task states have description.
        /// </summary>
        /// <param name="taskState">
        /// The task state.
        /// </param>
        [Test]
        public void TaskStateHasDescriptionTest([Values]TaskState taskState) => Assert.That(taskState.GetDescription(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all task states values are unique.
        /// </summary>
        [Test]
        public void TaskStateValuesUniqueTest() => Assert.That(taskStates.Cast<byte>(), Is.Unique);
    }
}
