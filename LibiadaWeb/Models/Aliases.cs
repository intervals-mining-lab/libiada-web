namespace LibiadaWeb.Models
{
    public static class Aliases
    {
        #region Language


        public static readonly int RussianLanguage = 11;
        public static readonly int EnglishLanguage = 12;
        public static readonly int GermanLanguage = 13;
        public static readonly int LanguageRussian = 11;
        public static readonly int LanguageEnglish = 12;
        public static readonly int LanguageGerman = 13;

        #endregion

        #region Applicability

        public static readonly int FullApplicability = 1;
        public static readonly int HomogeneousApplicability = 2;
        public static readonly int BinaryApplicability = 3;
        public static readonly int ApplicabilityFull = 1;
        public static readonly int ApplicabilityHomogeneous = 2;
        public static readonly int ApplicabilityBinary = 3;
        public static readonly int ApplicabilityFullHomogeneous = 4;
        public static readonly int ApplicabilityFullBinary = 5;
        public static readonly int ApplicabilityHomogeneousBinary = 6;
        public static readonly int ApplicabilityAll = 7;

        #endregion

        #region LinkUp

        public static readonly int LinkUpStart = 1;
        public static readonly int LinkUpEnd = 2;
        public static readonly int LinkUpStartEnd = 3;

        #endregion

        #region Nature

        public static readonly int NatureGenetic = 1;
        public static readonly int NatureMusic = 2;
        public static readonly int NatureLiterature = 3;

        #endregion

        #region Notation

        public static readonly int NotationNucleotide = 1;
        public static readonly int NotationTriplet = 2;
        public static readonly int NotationAminoAcid = 3;
        public static readonly int NotationSegmented = 6;
        public static readonly int NotationWords = 15;
        public static readonly int NotationFmotivs = 16;
        public static readonly int NotationMeasures = 18;
        public static readonly int NotationNotes = 19;

        #endregion

        #region NoteSymbol

        public static readonly int NoteSymbolA = 25;
        public static readonly int NoteSymbolB = 26;
        public static readonly int NoteSymbolC = 27;
        public static readonly int NoteSymbolD = 28;
        public static readonly int NoteSymbolE = 29;
        public static readonly int NoteSymbolF = 30;
        public static readonly int NoteSymbolG = 31;

        #endregion

    }
}