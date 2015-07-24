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
            /// Double flat accidental.
            /// </summary>
            public const int DoubleFlat = 1;

            /// <summary>
            /// Flat accidental.
            /// </summary>
            public const int Flat = 2;

            /// <summary>
            /// Bekar accidental.
            /// </summary>
            public const int Bekar = 3;

            /// <summary>
            /// Sharp accidental.
            /// </summary>
            public const int Sharp = 4;

            /// <summary>
            /// Double sharp accidental.
            /// </summary>
            public const int DoubleSharp = 5;
        }

        /// <summary>
        /// Sequence attributes.
        /// </summary>
        public static class Attribute
        {
            /// <summary>
            /// GenBank id.
            /// </summary>
            public const int DbXref = 1;

            /// <summary>
            /// The protein id.
            /// </summary>
            public const int ProteinId = 2;

            /// <summary>
            /// The complement flag.
            /// </summary>
            public const int Complement = 3;

            /// <summary>
            /// The complement join flag.
            /// </summary>
            public const int ComplementJoin = 4;

            /// <summary>
            /// The product name.
            /// </summary>
            public const int Product = 5;

            /// <summary>
            /// The note.
            /// </summary>
            public const int Note = 6;

            /// <summary>
            /// The codon start position (1, 2 or 3).
            /// </summary>
            public const int CodonStart = 7;

            /// <summary>
            /// The translation table for gene.
            /// </summary>
            public const int TranslationTable = 8;

            /// <summary>
            /// The inference.
            /// </summary>
            public const int Inference = 9;

            /// <summary>
            /// The repeat type.
            /// </summary>
            public const int RepeatType = 10;

            /// <summary>
            /// The locus tag.
            /// </summary>
            public const int LocusTag = 11;

            /// <summary>
            /// The old locus tag.
            /// </summary>
            public const int OldLocusTag = 12;

            /// <summary>
            /// The gene.
            /// </summary>
            public const int Gene = 13;

            /// <summary>
            /// The anticodon.
            /// </summary>
            public const int Anticodon = 14;

            /// <summary>
            /// The enzyme commission number.
            /// </summary>
            public const int EnzymeCommissionNumber = 15;

            /// <summary>
            /// The exception in annotation or translation.
            /// </summary>
            public const int Exception = 16;

            /// <summary>
            /// The gene synonym.
            /// </summary>
            public const int GeneSynonym = 17;

            /// <summary>
            /// The pseudo.
            /// </summary>
            public const int Pseudo = 18;

            /// <summary>
            /// The ncRNA class.
            /// </summary>
            public const int NcRnaClass = 19;

            /// <summary>
            /// The standart name.
            /// </summary>
            public const int StandartName = 20;

            /// <summary>
            /// The repeat family.
            /// </summary>
            public const int RepeatFamily = 21;

            /// <summary>
            /// The direction.
            /// </summary>
            public const int Direction = 22;

            /// <summary>
            /// The ribosomal slippage.
            /// </summary>
            public const int RibosomalSlippage = 23;

            /// <summary>
            /// The partial.
            /// </summary>
            public const int Partial = 24;

            /// <summary>
            /// The codon recognized.
            /// </summary>
            public const int CodonRecognized = 25;

            /// <summary>
            /// The bound moiety.
            /// </summary>
            public const int BoundMoiety = 26;

            /// <summary>
            /// The repeat unit range.
            /// </summary>
            public const int RepeatUnitRange = 27;

            /// <summary>
            /// The repeat unit sequence.
            /// </summary>
            public const int RepeatUnitSequence = 28;

            /// <summary>
            /// The function.
            /// </summary>
            public const int Function = 29;

            /// <summary>
            /// The translational exception.
            /// </summary>
            public const int TranslationalException = 30;

            /// <summary>
            /// The pseudogene.
            /// </summary>
            public const int Pseudogene = 31;

            /// <summary>
            /// The mobile element type.
            /// </summary>
            public const int MobileElementType = 32;
        }

        /// <summary>
        /// The language.
        /// </summary>
        public static class Language
        {
            /// <summary>
            /// Russian language.
            /// </summary>
            public const int Russian = 1;

            /// <summary>
            /// English language.
            /// </summary>
            public const int English = 2;

            /// <summary>
            /// German language.
            /// </summary>
            public const int German = 3;
        }

        /// <summary>
        /// The link.
        /// </summary>
        public static class Link
        {
            /// <summary>
            /// Link not applied.
            /// </summary>
            public const int NotApplied = 0;

            /// <summary>
            /// No link.
            /// </summary>
            public const int None = 1;

            /// <summary>
            /// The link to start.
            /// </summary>
            public const int Start = 2;

            /// <summary>
            /// The link to end.
            /// </summary>
            public const int End = 3;

            /// <summary>
            /// Start and end link.
            /// </summary>
            public const int StartEnd = 4;

            /// <summary>
            /// Cycle link.
            /// </summary>
            public const int Cycle = 5;
        }

        /// <summary>
        /// The nature.
        /// </summary>
        public static class Nature
        {
            /// <summary>
            /// The genetic nature.
            /// </summary>
            public const int Genetic = 1;

            /// <summary>
            /// The music nature.
            /// </summary>
            public const int Music = 2;

            /// <summary>
            /// The literature nature.
            /// </summary>
            public const int Literature = 3;

            /// <summary>
            /// The data nature.
            /// </summary>
            public const int Data = 4;
        }

        /// <summary>
        /// The notation.
        /// </summary>
        public static class Notation
        {
            /// <summary>
            /// Nucleotide notation.
            /// </summary>
            public const int Nucleotide = 1;

            /// <summary>
            /// The notation triplet.
            /// </summary>
            public const int Triplet = 2;

            /// <summary>
            /// Amino acid notation.
            /// </summary>
            public const int AminoAcid = 3;

            /// <summary>
            /// Segmented dna notation.
            /// </summary>
            public const int Segmented = 4;

            /// <summary>
            /// Words notation.
            /// </summary>
            public const int Words = 5;

            /// <summary>
            /// Fmotives notation.
            /// </summary>
            public const int Fmotives = 6;

            /// <summary>
            /// Measures notation.
            /// </summary>
            public const int Measures = 7;

            /// <summary>
            /// Notes notation.
            /// </summary>
            public const int Notes = 8;

            /// <summary>
            /// Letters The notation.
            /// </summary>
            public const int Letters = 9;

            /// <summary>
            /// The integer values.
            /// </summary>
            public const int IntegerValues = 10;

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
            /// The note symbol A.
            /// </summary>
            public const int A = 1;

            /// <summary>
            /// The note symbol B.
            /// </summary>
            public const int B = 2;

            /// <summary>
            /// The note symbol C.
            /// </summary>
            public const int C = 3;

            /// <summary>
            /// The note symbol D.
            /// </summary>
            public const int D = 4;

            /// <summary>
            /// The note symbol E.
            /// </summary>
            public const int E = 5;

            /// <summary>
            /// The note symbol F.
            /// </summary>
            public const int F = 6;

            /// <summary>
            /// The note symbol G.
            /// </summary>
            public const int G = 7;
        }

        /// <summary>
        /// The feature.
        /// </summary>
        public static class Feature
        {
            /// <summary>
            /// The feature full genome.
            /// </summary>
            public const int FullGenome = 1;

            /// <summary>
            /// The feature full text.
            /// </summary>
            public const int FullText = 2;

            /// <summary>
            /// The feature full song.
            /// </summary>
            public const int FullSong = 3;

            /// <summary>
            /// The feature coding sequence.
            /// </summary>
            public const int CodingSequence = 4;

            /// <summary>
            /// The Ribosomal RNA feature.
            /// </summary>
            public const int RibosomalRNA = 5;

            /// <summary>
            /// The Transfer RNA feature.
            /// </summary>
            public const int TransferRNA = 6;

            /// <summary>
            /// The feature Non-coding RNA.
            /// </summary>
            public const int NoncodingRNA = 7;

            /// <summary>
            /// The feature Transfer-messenger RNA.
            /// </summary>
            public const int TransferMessengerRNA = 8;

            /// <summary>
            /// The feature pseudo gen.
            /// </summary>
            public const int PseudoGen = 9;

            /// <summary>
            /// The feature plasmid.
            /// </summary>
            public const int Plasmid = 10;

            /// <summary>
            /// The feature mitochondrion genome.
            /// </summary>
            public const int MitochondrionGenome = 11;

            /// <summary>
            /// The feature mitochondrion ribosomal rna.
            /// </summary>
            public const int MitochondrionRRNA = 12;

            /// <summary>
            /// The feature repeat region.
            /// </summary>
            public const int RepeatRegion = 13;

            /// <summary>
            /// The feature non coding sequence.
            /// </summary>
            public const int NonCodingSequence = 14;

            /// <summary>
            /// The feature chloroplast genome.
            /// </summary>
            public const int ChloroplastGenome = 15;

            /// <summary>
            /// The feature Miscellaneous other RNA.
            /// </summary>
            public const int MiscellaneousOtherRNA = 16;

            /// <summary>
            /// The complete numeric sequence feature.
            /// </summary>
            public const int CompleteNumericSequence = 17;

            /// <summary>
            /// The miscellaneous feature.
            /// </summary>
            public const int MiscellaneousFeature = 18;

            /// <summary>
            /// The Messenger RNA feature.
            /// </summary>
            public const int MessengerRNA = 19;

            /// <summary>
            /// The regulatory feature.
            /// </summary>
            public const int Regulatory = 20;

            /// <summary>
            /// The sequence tagged site.
            /// </summary>
            public const int SequenceTaggedSite = 21;

            /// <summary>
            /// The origin of replication.
            /// </summary>
            public const int OriginOfReplication = 22;

            /// <summary>
            /// The signal peptide coding sequence.
            /// </summary>
            public const int SignalPeptideCodingSequence = 23;

            /// <summary>
            /// The miscellaneous binding.
            /// </summary>
            public const int MiscellaneousBinding = 24;
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

        /// <summary>
        /// The output formats.
        /// </summary>
        public static class OutputFormats
        {
            /// <summary>
            /// The date time output format.
            /// </summary>
            public const string DateTimeFormat = "dd MMMM yyyy HH':'mm':'ss";

            /// <summary>
            /// The time output format.
            /// </summary>
            public const string TimeFormat = @"hh\:mm\:ss\.FF";
        }
    }
}
