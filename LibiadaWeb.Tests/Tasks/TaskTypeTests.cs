﻿namespace LibiadaWeb.Tests.Tasks
{
    using System.Linq;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Tasks;

    using NUnit.Framework;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// TaskType enum tests.
    /// </summary>
    [TestFixture(TestOf = typeof(TaskType))]
    public class TaskTypeTests
    {
        /// <summary>
        /// The task types count.
        /// </summary>
        private const int TaskTypesCount = 39;

        /// <summary>
        /// Array of all tasks types.
        /// </summary>
        private readonly TaskType[] taskTypes = EnumExtensions.ToArray<TaskType>();

        /// <summary>
        /// Tests count of task types.
        /// </summary>
        [Test]
        public void TaskTypeCountTest() => Assert.AreEqual(TaskTypesCount, taskTypes.Length);

        /// <summary>
        /// Tests values of task types.
        /// </summary>
        [Test]
        public void TaskTypeValuesTest()
        {
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
        [TestCase((TaskType)7, "MusicFiles")]
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
        [TestCase((TaskType)25, "SequencePrediction")]
        [TestCase((TaskType)26, "BatchPoemsImport")]
        [TestCase((TaskType)27, "OrderCalculation")]
        [TestCase((TaskType)28, "OrderTransformationConvergence")]
        [TestCase((TaskType)29, "BatchMusicImport")]
        [TestCase((TaskType)30, "OrderTransformationVisualization")]
        [TestCase((TaskType)31, "FmotifsDictionary")]
        [TestCase((TaskType)32, "OrderTransformationCharacteristicsDynamicVisualization")]
        [TestCase((TaskType)33, "OrdersIntervalsDistributionsAccordance")]
        [TestCase((TaskType)34, "IntervalsCharacteristicsDistribution")]
        [TestCase((TaskType)35, "BatchImagesImport")]
        [TestCase((TaskType)36, "BatchGeneticImportFromGenBankSearchFile")]
        [TestCase((TaskType)37, "BatchGeneticImportFromGenBankSearchQuery")]
        [TestCase((TaskType)38, "NcbiNuccoreSearch")]
        [TestCase((TaskType)39, "GenBankAccessionVersionUpdateChecker")]
        public void TaskTypeNameTest(TaskType taskType, string name) => Assert.AreEqual(name, taskType.GetName());

        /// <summary>
        /// Tests that all task types have display value.
        /// </summary>
        /// <param name="taskType">
        /// The task type.
        /// </param>
        [Test]
        public void TaskTypeHasDisplayValueTest([Values]TaskType taskType) => Assert.That(taskType.GetDisplayValue(), Is.Not.Null.And.Not.Empty);

        /// <summary>
        /// Tests that all task types have display value.
        /// </summary>
        /// <param name="taskType">
        /// The task type.
        /// </param>
        [Test]
        public void TaskTypeHasTaskClassAttributeTest([Values]TaskType taskType) => Assert.IsNotNull(taskType.GetTaskClass());

        /// <summary>
        /// Tests that all task types values are unique.
        /// </summary>
        [Test]
        public void TaskTypeValuesUniqueTest() => Assert.That(taskTypes.Cast<byte>(), Is.Unique);
    }
}
