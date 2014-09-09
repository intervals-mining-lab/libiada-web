namespace LibiadaWeb.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The aliases.
    /// </summary>
    public static class Aliases
    {
        #region Accidental

        /// <summary>
        /// The accidental double flat.
        /// </summary>
        public const int AccidentalDoubleFlat = 1;

        /// <summary>
        /// The accidental flat.
        /// </summary>
        public const int AccidentalFlat = 2;

        /// <summary>
        /// The accidental bekar.
        /// </summary>
        public const int AccidentalBekar = 3;

        /// <summary>
        /// The accidental sharp.
        /// </summary>
        public const int AccidentalSharp = 4;

        /// <summary>
        /// The accidental double sharp.
        /// </summary>
        public const int AccidentalDoubleSharp = 5;

        #endregion

        #region Language

        /// <summary>
        /// The language russian.
        /// </summary>
        public const int LanguageRussian = 1;

        /// <summary>
        /// The language english.
        /// </summary>
        public const int LanguageEnglish = 2;

        /// <summary>
        /// The language german.
        /// </summary>
        public const int LanguageGerman = 3;

        #endregion

        #region Link

        /// <summary>
        /// The link start.
        /// </summary>
        public const int LinkStart = 1;

        /// <summary>
        /// The link end.
        /// </summary>
        public const int LinkEnd = 2;

        /// <summary>
        /// The link start end.
        /// </summary>
        public const int LinkStartEnd = 3;

        /// <summary>
        /// The link cycle.
        /// </summary>
        public const int LinkCycle = 4;

        /// <summary>
        /// The link none.
        /// </summary>
        public const int LinkNone = 5;

        #endregion

        #region Nature

        /// <summary>
        /// The nature genetic.
        /// </summary>
        public const int NatureGenetic = 1;

        /// <summary>
        /// The nature music.
        /// </summary>
        public const int NatureMusic = 2;

        /// <summary>
        /// The nature literature.
        /// </summary>
        public const int NatureLiterature = 3;

        #endregion

        #region Notation

        /// <summary>
        /// The notation nucleotide.
        /// </summary>
        public const int NotationNucleotide = 1;

        /// <summary>
        /// The notation triplet.
        /// </summary>
        public const int NotationTriplet = 2;

        /// <summary>
        /// The notation amino acid.
        /// </summary>
        public const int NotationAminoAcid = 3;

        /// <summary>
        /// The notation segmented.
        /// </summary>
        public const int NotationSegmented = 4;

        /// <summary>
        /// The notation words.
        /// </summary>
        public const int NotationWords = 5;

        /// <summary>
        /// The notation fmotivs.
        /// </summary>
        public const int NotationFmotivs = 6;

        /// <summary>
        /// The notation measures.
        /// </summary>
        public const int NotationMeasures = 7;

        /// <summary>
        /// The notation notes.
        /// </summary>
        public const int NotationNotes = 8;

        /// <summary>
        /// The notation letters.
        /// </summary>
        public const int NotationLetters = 9;

        /// <summary>
        /// The static notations.
        /// </summary>
        public static readonly List<int> StaticNotations = new List<int> { NotationNucleotide, NotationTriplet, NotationAminoAcid, NotationLetters }; 

        #endregion

        #region NoteSymbol

        /// <summary>
        /// The note symbol a.
        /// </summary>
        public const int NoteSymbolA = 1;

        /// <summary>
        /// The note symbol b.
        /// </summary>
        public const int NoteSymbolB = 2;

        /// <summary>
        /// The note symbol c.
        /// </summary>
        public const int NoteSymbolC = 3;

        /// <summary>
        /// The note symbol d.
        /// </summary>
        public const int NoteSymbolD = 4;

        /// <summary>
        /// The note symbol e.
        /// </summary>
        public const int NoteSymbolE = 5;

        /// <summary>
        /// The note symbol f.
        /// </summary>
        public const int NoteSymbolF = 6;

        /// <summary>
        /// The note symbol g.
        /// </summary>
        public const int NoteSymbolG = 7;

        #endregion

        #region PieceType

        /// <summary>
        /// The piece type full genome.
        /// </summary>
        public const int PieceTypeFullGenome = 1;

        /// <summary>
        /// The piece type full text.
        /// </summary>
        public const int PieceTypeFullText = 2;

        /// <summary>
        /// The piece type full song.
        /// </summary>
        public const int PieceTypeFullSong = 3;

        /// <summary>
        /// The piece type coding sequence.
        /// </summary>
        public const int PieceTypeCodingSequence = 4;

        /// <summary>
        /// The piece type rrna.
        /// </summary>
        public const int PieceTypeRRNA = 5;

        /// <summary>
        /// The piece type trna.
        /// </summary>
        public const int PieceTypeTRNA = 6;

        /// <summary>
        /// The piece type ncrna.
        /// </summary>
        public const int PieceTypeNCRNA = 7;

        /// <summary>
        /// The piece type tmrna.
        /// </summary>
        public const int PieceTypeTMRNA = 8;

        /// <summary>
        /// The piece type pseudo gen.
        /// </summary>
        public const int PieceTypePseudoGen = 9;

        /// <summary>
        /// The piece type plasmid.
        /// </summary>
        public const int PieceTypePlasmid = 10;

        /// <summary>
        /// The piece type mitochondrion genome.
        /// </summary>
        public const int PieceTypeMitochondrionGenome = 11;

        /// <summary>
        /// The piece type mitochondrion rrna.
        /// </summary>
        public const int PieceTypeMitochondrionRRNA = 12;

        /// <summary>
        /// The piece type repeat region.
        /// </summary>
        public const int PieceTypeRepeatRegion = 13;

        /// <summary>
        /// The piece type non coding sequence.
        /// </summary>
        public const int PieceTypeNonCodingSequence = 14;

        /// <summary>
        /// The piece type chloroplast genome.
        /// </summary>
        public const int PieceTypeChloroplastGenome = 15;

        #endregion

        #region RemoteDb

        /// <summary>
        /// The remote db ncbi.
        /// </summary>
        public const int RemoteDbNcbi = 1;

        #endregion
    }
}