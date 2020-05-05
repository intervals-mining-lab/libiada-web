using System;

namespace LibiadaWeb.Tests
{
    using LibiadaWeb.Models.Repositories.Sequences;

    using NUnit.Framework;

    [TestFixture]
    class SequenceRepositoryTests
    {
        private static readonly string[] segmentTestData =
        {
          "Orchid fleck dichorhavirus | Orchid fleck virus genomic RNA, segment RNA 1 | NC_009608.1",
          "Yellowtail ascites virus segment A | NC_004168.1",
          "Influenza A virus (A/New York/392/2004(H3N2)) segment 6 | CY002066.1"
        };

        private static readonly string[] chromosomeTestData =
        {
            "Burkholderia mallei ATCC 23344 chromosome 1 | CP000010.1",
            "Brucella abortus biovar 1 str. 9-941 chromosome II | AE017224.1",
            "Lactobacillus acidophilus NCFM chromosome | NC_006814.3"
        };

        private static readonly string[] plasmidTestData =
        {
            "Prosthecochloris aestuarii DSM 271 plasmid pPAES01 | CP001109.1",
            "Lactobacillus paracasei subsp. paracasei 8700:2 plasmid 1 | CP002392.1",
            "Nitrosococcus oceani ATCC 19707 plasmid A | CP000126.1"
        };


        [Test]
        public void GetSequenceSegmentNumberTest()
        {
            string result = "";
            string expected = "116";
            foreach (var record in segmentTestData)
            {
                result += MultisequenceRepository.GetSequenceNumberByName(MultisequenceRepository.GetMatterNameSplit(record));
            }
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetSequenceChromosomeNumberTest()
        {
            string result = "";
            string expected = "120";
            foreach (var record in chromosomeTestData)
            {
                result += MultisequenceRepository.GetSequenceNumberByName(MultisequenceRepository.GetMatterNameSplit(record));
            }
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetSequencePlasmidNumberTest()
        {
            string result = "";
            string expected = "111";
            foreach (var record in plasmidTestData)
            {
                result += MultisequenceRepository.GetSequenceNumberByName(MultisequenceRepository.GetMatterNameSplit(record));
            }
            Assert.AreEqual(expected, result);
        }

    }
}
