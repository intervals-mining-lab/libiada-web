namespace LibiadaWeb.Models
{
    public static class Aliases
    {
        #region Accidental

        public const int AccidentalDoubleFlat = 1;
        public const int AccidentalFlat = 2;
        public const int AccidentalBekar = 3;
        public const int AccidentalSharp = 4;
        public const int AccidentalDoubleSharp = 5;

        #endregion

        #region Language

        public const int LanguageRussian = 1;
        public const int LanguageEnglish = 2;
        public const int LanguageGerman = 3;

        #endregion

        #region Link

        public const int LinkStart = 1;
        public const int LinkEnd = 2;
        public const int LinkStartEnd = 3;
        public const int LinkCycle = 4;
        public const int LinkNone = 5;

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
        public const int PieceTypeFullSong = 3;
        public const int PieceTypeCodingSequence = 4;
        public const int PieceTypeTRNA = 5;
        public const int PieceTypeRRNA = 6;
        public const int PieceTypeNCRNA = 7;
        public const int PieceTypeTMRNA = 8;
        public const int PieceTypePseudoGen = 9;

        #endregion

        #region RemoteDb

        public const int RemoteDbNcbi = 1;

        #endregion

    }
}