using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Diagnostics;
using LibiadaWeb.Helpers;
using NUnit.Framework;

namespace LibiadaWeb.Tests.Helpers
{
    [TestFixture(TestOf = typeof(NcbiHelper))]

   public class NcbiHelperTests
    {
        public string Text = @"
1. Tomato yellow leaf curl virus - Israel replication-associated protein gene, partial cds
600 bp linear DNA 
AY647456.1 GI:49823161

2. Tomato yellow leaf curl virus - IL [JR:Osaka] DNA, complete genome
2,781 bp circular DNA 
LC099965.1 GI:958114347

3. UNVERIFIED: Tomato yellow leaf curl virus isolate YNC AC3 protein-like gene, complete sequence; and AC2 protein-like gene, partial sequence
505 bp linear DNA 
KC684958.1 GI:514400153";
      
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
            var result = NcbiHelper.GetIdFromFile(textFromFile, true,5000,100000);
            int expectedCountOfSequences = 122;
            Assert.AreEqual(expectedCountOfSequences, result.Length);
        }
    }
}
