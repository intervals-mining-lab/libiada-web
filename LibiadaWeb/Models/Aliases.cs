using System.Collections.Generic;

namespace LibiadaWeb.Models
{
    public static class Aliases
    {
        #region Language

        public const int LanguageRussian = 11;
        public const int LanguageEnglish = 12;
        public const int LanguageGerman = 13;

        #endregion

        #region Applicability

        public const int ApplicabilityOnlyFull = 1;
        public const int ApplicabilityOnlyCongeneric = 2;
        public const int ApplicabilityOnlyBinary = 3;
        public const int ApplicabilityFullCongeneric = 4;
        public const int ApplicabilityFullBinary = 5;
        public const int ApplicabilityCongenericBinary = 6;
        public const int ApplicabilityAll = 7;

        public static readonly List<int> ApplicabilityFull = new List<int> 
        { ApplicabilityOnlyFull, ApplicabilityFullCongeneric, ApplicabilityFullBinary, ApplicabilityAll };

        public static readonly List<int> ApplicabilityCongeneric = new List<int> 
        { ApplicabilityOnlyCongeneric, ApplicabilityFullCongeneric, ApplicabilityCongenericBinary, ApplicabilityAll };

        public static readonly List<int> ApplicabilityBinary = new List<int> 
        { ApplicabilityOnlyBinary, ApplicabilityFullBinary, ApplicabilityCongenericBinary, ApplicabilityAll };


        #endregion

        #region LinkUp

        public const int LinkUpStart = 1;
        public const int LinkUpEnd = 2;
        public const int LinkUpStartEnd = 3;
        public const int LinkUpCycle = 4;
        public const int LinkUpNone = 5;

        #endregion

        #region Nature

        public const int NatureGenetic = 1;
        public const int NatureMusic = 2;
        public const int NatureLiterature = 3;

        #endregion

        #region Notation

        public const int NotationNucleotide = 1;
        public const int NotationTriplet = 2;
        public const int NotationAminoAcid = 3;
        public const int NotationSegmented = 6;
        public const int NotationWords = 15;
        public const int NotationFmotivs = 16;
        public const int NotationMeasures = 18;
        public const int NotationNotes = 19;

        #endregion

        #region NoteSymbol

        public const int NoteSymbolA = 25;
        public const int NoteSymbolB = 26;
        public const int NoteSymbolC = 27;
        public const int NoteSymbolD = 28;
        public const int NoteSymbolE = 29;
        public const int NoteSymbolF = 30;
        public const int NoteSymbolG = 31;

        #endregion

    }
}