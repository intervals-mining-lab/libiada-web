namespace LibiadaWeb.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The aliases.
    /// </summary>
    public static class Aliases
    {
        /// <summary>
        /// The accidental.
        /// </summary>
        public static class Accidental
        {
            /// <summary>
            /// The accidental double flat.
            /// </summary>
            public const int DoubleFlat = 1;

            /// <summary>
            /// The accidental flat.
            /// </summary>
            public const int Flat = 2;

            /// <summary>
            /// The accidental bekar.
            /// </summary>
            public const int Bekar = 3;

            /// <summary>
            /// The accidental sharp.
            /// </summary>
            public const int Sharp = 4;

            /// <summary>
            /// The accidental double sharp.
            /// </summary>
            public const int DoubleSharp = 5;
        }

        /// <summary>
        /// The language.
        /// </summary>
        public static class Language
        {
            /// <summary>
            /// The language russian.
            /// </summary>
            public const int Russian = 1;

            /// <summary>
            /// The language english.
            /// </summary>
            public const int English = 2;

            /// <summary>
            /// The language german.
            /// </summary>
            public const int German = 3;
        }

        /// <summary>
        /// The link.
        /// </summary>
        public static class Link
        {
            /// <summary>
            /// The link none.
            /// </summary>
            public const int None = 0;

            /// <summary>
            /// The link start.
            /// </summary>
            public const int Start = 1;

            /// <summary>
            /// The link end.
            /// </summary>
            public const int End = 2;

            /// <summary>
            /// The link start end.
            /// </summary>
            public const int StartEnd = 3;

            /// <summary>
            /// The link cycle.
            /// </summary>
            public const int Cycle = 4;
        }

        /// <summary>
        /// The nature.
        /// </summary>
        public static class Nature
        {
            /// <summary>
            /// The nature genetic.
            /// </summary>
            public const int Genetic = 1;

            /// <summary>
            /// The nature music.
            /// </summary>
            public const int Music = 2;

            /// <summary>
            /// The nature literature.
            /// </summary>
            public const int Literature = 3;
        }

        /// <summary>
        /// The notation.
        /// </summary>
        public static class Notation
        {
            /// <summary>
            /// The notation nucleotide.
            /// </summary>
            public const int Nucleotide = 1;

            /// <summary>
            /// The notation triplet.
            /// </summary>
            public const int Triplet = 2;

            /// <summary>
            /// The notation amino acid.
            /// </summary>
            public const int AminoAcid = 3;

            /// <summary>
            /// The notation segmented.
            /// </summary>
            public const int Segmented = 4;

            /// <summary>
            /// The notation words.
            /// </summary>
            public const int Words = 5;

            /// <summary>
            /// The notation fmotives.
            /// </summary>
            public const int Fmotives = 6;

            /// <summary>
            /// The notation measures.
            /// </summary>
            public const int Measures = 7;

            /// <summary>
            /// The notation notes.
            /// </summary>
            public const int Notes = 8;

            /// <summary>
            /// The notation letters.
            /// </summary>
            public const int Letters = 9;

            /// <summary>
            /// Notations elements of which will not change.
            /// </summary>
            public static readonly List<int> StaticNotations = new List<int> { Nucleotide, Triplet, AminoAcid, Letters };
        }

        /// <summary>
        /// The note symbol.
        /// </summary>
        public static class NoteSymbol
        {
            /// <summary>
            /// The note symbol a.
            /// </summary>
            public const int A = 1;

            /// <summary>
            /// The note symbol b.
            /// </summary>
            public const int B = 2;

            /// <summary>
            /// The note symbol c.
            /// </summary>
            public const int C = 3;

            /// <summary>
            /// The note symbol d.
            /// </summary>
            public const int D = 4;

            /// <summary>
            /// The note symbol e.
            /// </summary>
            public const int E = 5;

            /// <summary>
            /// The note symbol f.
            /// </summary>
            public const int F = 6;

            /// <summary>
            /// The note symbol g.
            /// </summary>
            public const int G = 7;
        }

        /// <summary>
        /// The piece type.
        /// </summary>
        public static class PieceType
        {
            /// <summary>
            /// The piece type full genome.
            /// </summary>
            public const int FullGenome = 1;

            /// <summary>
            /// The piece type full text.
            /// </summary>
            public const int FullText = 2;

            /// <summary>
            /// The piece type full song.
            /// </summary>
            public const int FullSong = 3;

            /// <summary>
            /// The piece type coding sequence.
            /// </summary>
            public const int CodingSequence = 4;

            /// <summary>
            /// The piece type rrna.
            /// </summary>
            public const int RRNA = 5;

            /// <summary>
            /// The piece type trna.
            /// </summary>
            public const int TRNA = 6;

            /// <summary>
            /// The piece type ncrna.
            /// </summary>
            public const int NCRNA = 7;

            /// <summary>
            /// The piece type tmrna.
            /// </summary>
            public const int TMRNA = 8;

            /// <summary>
            /// The piece type pseudo gen.
            /// </summary>
            public const int PseudoGen = 9;

            /// <summary>
            /// The piece type plasmid.
            /// </summary>
            public const int Plasmid = 10;

            /// <summary>
            /// The piece type mitochondrion genome.
            /// </summary>
            public const int MitochondrionGenome = 11;

            /// <summary>
            /// The piece type mitochondrion rrna.
            /// </summary>
            public const int MitochondrionRRNA = 12;

            /// <summary>
            /// The piece type repeat region.
            /// </summary>
            public const int RepeatRegion = 13;

            /// <summary>
            /// The piece type non coding sequence.
            /// </summary>
            public const int NonCodingSequence = 14;

            /// <summary>
            /// The piece type chloroplast genome.
            /// </summary>
            public const int ChloroplastGenome = 15;

            /// <summary>
            /// The piece type misc rna.
            /// </summary>
            public const int MiscRNA = 16;
        }

        /// <summary>
        /// The remote db.
        /// </summary>
        public static class RemoteDb
        {
            /// <summary>
            /// The remote db ncbi.
            /// </summary>
            public const int RemoteDbNcbi = 1;
        }
    }
}
