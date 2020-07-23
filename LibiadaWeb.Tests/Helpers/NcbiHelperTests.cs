using System.IO;
using LibiadaWeb.Helpers;
using NUnit.Framework;

namespace LibiadaWeb.Tests.Helpers
{
    [TestFixture(TestOf = typeof(NcbiHelper))]
    public class NcbiHelperTests
    {

        [Test]
        public void GetIDFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true);
            int expectedSequencesCount = 2111;
            Assert.AreEqual(expectedSequencesCount, result.Length);
        }

        [Test]
        public void IncludePartialInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false);
            int expectedSequencesCount = 1447;
            int partialSequences = 823;
            expectedSequencesCount -= partialSequences;
            Assert.AreEqual(expectedSequencesCount, result.Length);
        }

        [Test]
        public void LengthInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, 5000, 100000);
            int expectedSequencesCount = 122;
            Assert.AreEqual(expectedSequencesCount, result.Length);
        }

        [Test]
        public void LengthPartialFalseInGetIdFromFileWithTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, 5000, 100000);
            int expectedSequencesCount = 121;
            Assert.AreEqual(expectedSequencesCount, result.Length);
        }

        [Test]
        public void MaxLengthPartialFalseInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, maxLength: 10000);
            int expectedSequencesCount = 507;
            Assert.AreEqual(expectedSequencesCount, result.Length);
        }

        [Test]
        public void MaxLengthPartialTrueInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, maxLength: 10000);
            int expectedSequencesCount = 1330;
            Assert.AreEqual(expectedSequencesCount, result.Length);
        }

        [Test]
        public void MinLengthPartialTrueInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, true, minLength: 30000);
            int expectedSequencesCount = 115;
            Assert.AreEqual(expectedSequencesCount, result.Length);
        }
        [Test]
        public void MinLengthPartialFalseInGetIdFromFileTest()
        {
            var txtReader = new StreamReader($"{SystemData.ProjectFolderPathForNcbiHelper}nuccore_result2.txt");
            var textFromFile = txtReader.ReadToEnd();
            var result = NcbiHelper.GetIdsFromNcbiSearchResults(textFromFile, false, minLength: 1000);
            int expectedSequencesCount = 415;
            Assert.AreEqual(expectedSequencesCount, result.Length);
        }
    }
}
