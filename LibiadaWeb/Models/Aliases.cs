using System.Collections.Generic;

namespace LibiadaWeb.Models
{
    public static class Aliases
    {
        #region Language

        public const int LanguageRussian = 1;
        public const int LanguageEnglish = 2;
        public const int LanguageGerman = 3;

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
        public const int NotationSegmented = 4;
        public const int NotationWords = 5;
        public const int NotationFmotivs = 6;
        public const int NotationMeasures = 7;
        public const int NotationNotes = 8;
        public const int NotationLetters = 9;

        #endregion

        #region NoteSymbol

        public const int NoteSymbolA = 1;
        public const int NoteSymbolB = 2;
        public const int NoteSymbolC = 3;
        public const int NoteSymbolD = 4;
        public const int NoteSymbolE = 5;
        public const int NoteSymbolF = 6;
        public const int NoteSymbolG = 7;

        #endregion

        #region PieceType

        public const int PieceTypeFullGenome = 1;
        public const int PieceTypeFullText = 2;
        public const int PieceTypeFuulSong = 3;

        #endregion

    }
}