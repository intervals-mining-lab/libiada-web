namespace LibiadaWeb.Tests.Tasks
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Attributes;
    using LibiadaWeb.Tasks;

    using NUnit.Framework;

    /// <summary>
    /// TaskType enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(TaskType))]
    public class TaskTypeTests
    {
        /// <summary>
        /// The task types count.
        /// </summary>
        private const int TaskTypesCount = 24;

        /// <summary>
        /// Tests count of task types.
        /// </summary>
        [Test]
        public void TaskTypeCountTest()
        {
            int actualCount = EnumExtensions.ToArray<TaskType>().Length;
            Assert.AreEqual(TaskTypesCount, actualCount);
        }

        /// <summary>
        /// Tests values of task types.
        /// </summary>
        [Test]
        public void TaskTypeValuesTest()
        {
            TaskType[] taskTypes = EnumExtensions.ToArray<TaskType>();

            for (int i = 1; i <= TaskTypesCount; i++)
            {
                Assert.IsTrue(taskTypes.Contains((TaskType)i));
            }
        }

        /// <summary>
        /// Tests names of task types.
        /// </summary>
        /// <param name="taskType">
        /// The task type.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        [TestCase((TaskType)1, "AccordanceCalculation")]
        [TestCase((TaskType)2, "Calculation")]
        [TestCase((TaskType)3, "Clusterization")]
        [TestCase((TaskType)4, "CongenericCalculation")]
        [TestCase((TaskType)5, "CustomSequenceCalculation")]
        [TestCase((TaskType)6, "CustomSequenceOrderTransformationCalculation")]
        [TestCase((TaskType)7, "FilteredSubsequenceCalculation")]
        [TestCase((TaskType)8, "LocalCalculation")]
        [TestCase((TaskType)9, "OrderTransformationCalculation")]
        [TestCase((TaskType)10, "RelationCalculation")]
        [TestCase((TaskType)11, "SequencesAlignment")]
        [TestCase((TaskType)12, "SubsequencesCalculation")]
        [TestCase((TaskType)13, "SubsequencesComparer")]
        [TestCase((TaskType)14, "SubsequencesDistribution")]
        [TestCase((TaskType)15, "SubsequencesSimilarity")]
        [TestCase((TaskType)16, "SequencesOrderDistribution")]
        [TestCase((TaskType)17, "BatchGenesImport")]
        [TestCase((TaskType)18, "BatchSequenceImport")]
        [TestCase((TaskType)19, "CustomSequenceOrderTransformer")]
        [TestCase((TaskType)20, "GenesImport")]
        [TestCase((TaskType)21, "OrderTransformer")]
        [TestCase((TaskType)22, "SequenceCheck")]
        [TestCase((TaskType)23, "CommonSequences")]
        [TestCase((TaskType)24, "Matters")]
        public void TaskTypeNameTest(TaskType taskType, string name)
        {
            Assert.AreEqual(name, taskType.GetName());
        }

        /// <summary>
        /// Tests that all task types have display value.
        /// </summary>
        /// <param name="taskType">
        /// The task type.
        /// </param>
        [Test]
        public void TaskTypeHasDisplayValueTest([Values]TaskType taskType)
        {
            Assert.IsFalse(string.IsNullOrEmpty(taskType.GetDisplayValue()));
        }

        /// <summary>
        /// Tests that all task types have display value.
        /// </summary>
        /// <param name="taskType">
        /// The task type.
        /// </param>
        [Test]
        public void TaskTypeHasTaskClassAttributeTest([Values]TaskType taskType)
        {
            Assert.IsNotNull(taskType.GetAttribute<TaskType, TaskClassAttribute>().Value);
        }

        /// <summary>
        /// Tests that all task types values are unique.
        /// </summary>
        [Test]
        public void TaskTypeValuesUniqueTest()
        {
            TaskType[] taskTypes = EnumExtensions.ToArray<TaskType>();
            Assert.That(taskTypes.Cast<byte>(), Is.Unique);
        }
    }
}
