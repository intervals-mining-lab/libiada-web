namespace LibiadaWeb.Tests.Helpers
{
    using System.IO;
    using LibiadaWeb.Helpers;
    using NUnit.Framework;

    [TestFixture(TestOf = typeof(NcbiHelper))]
    public class NcbiHelperTests
    {

        [Test]
        public void GetIDFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdFromFile(textFromFile, true);
            int expectedCountOfSequences = 2111;
            Assert.AreEqual(expectedCountOfSequences, result.Length);
        }

        [Test]
        public void IncludePartialInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdFromFile(textFromFile, false);
            int expectedCountOfSequences = 1447;
            int partialSequences = 823;
            expectedCountOfSequences = expectedCountOfSequences - partialSequences;
            Assert.AreEqual(expectedCountOfSequences, result.Length);
        }

        [Test]
        public void LengthInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdFromFile(textFromFile, true, 5000, 100000);
            int expectedCountOfSequences = 122;
            Assert.AreEqual(expectedCountOfSequences, result.Length);
        }

        [Test]
        public void LengthPartialFalseInGetIdFromFileWithTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdFromFile(textFromFile, false, 5000, 100000);
            int expectedCountOfSequences = 121;
            Assert.AreEqual(expectedCountOfSequences, result.Length);
        }

        [Test]
        public void MaxLengthPartialFalseInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdFromFile(textFromFile, false, maxLength: 10000);
            int expectedCountOfSequences = 507;
            Assert.AreEqual(expectedCountOfSequences, result.Length);
        }

        [Test]
        public void MaxLengthPartialTrueInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdFromFile(textFromFile, true, maxLength: 10000);
            int expectedCountOfSequences = 1330;
            Assert.AreEqual(expectedCountOfSequences, result.Length);
        }

        [Test]
        public void MinLengthPartialTrueInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdFromFile(textFromFile, true, minLength: 30000);
            int expectedCountOfSequences = 115;
            Assert.AreEqual(expectedCountOfSequences, result.Length);
        }
    }
}
