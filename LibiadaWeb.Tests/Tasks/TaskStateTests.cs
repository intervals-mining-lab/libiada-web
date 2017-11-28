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
        /// Tests count of task states.
        /// </summary>
        [Test]
        public void TaskStateCountTest()
        {
            int actualCount = EnumExtensions.ToArray<TaskState>().Length;
            Assert.AreEqual(TaskStatesCount, actualCount);
        }

        /// <summary>
        /// Tests values of task states.
        /// </summary>
        [Test]
        public void TaskStateValuesTest()
        {
            TaskState[] taskStates = EnumExtensions.ToArray<TaskState>();

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
        public void TaskStateNameTest(TaskState taskState, string name)
        {
            Assert.AreEqual(name, taskState.GetName());
        }

        /// <summary>
        /// Tests that all task states have display value.
        /// </summary>
        /// <param name="taskState">
        /// The task state.
        /// </param>
        [Test]
        public void TaskStateHasDisplayValueTest([Values]TaskState taskState)
        {
            Assert.IsFalse(string.IsNullOrEmpty(taskState.GetDisplayValue()));
        }

        /// <summary>
        /// Tests that all task states have description.
        /// </summary>
        /// <param name="taskState">
        /// The task state.
        /// </param>
        [Test]
        public void TaskStateHasDescriptionTest([Values]TaskState taskState)
        {
            Assert.IsFalse(string.IsNullOrEmpty(taskState.GetDescription()));
        }

        /// <summary>
        /// Tests that all task states values are unique.
        /// </summary>
        [Test]
        public void TaskStateValuesUniqueTest()
        {
            TaskState[] taskStates = EnumExtensions.ToArray<TaskState>();
            Assert.That(taskStates.Cast<byte>(), Is.Unique);
        }
    }
}
