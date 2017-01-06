namespace LibiadaWeb.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The aliases.
    /// </summary>
    public static class Aliases
    {
        /// <summary>
        /// The user available characteristics.
        /// </summary>
        public static readonly ReadOnlyCollection<CharacteristicType> UserAvailableCharacteristics = new ReadOnlyCollection<CharacteristicType>(new List<CharacteristicType>
                                                                                                                                          {
                                                                                                                                            CharacteristicType.ATSkew,
                                                                                                                                            CharacteristicType.AlphabetCardinality,
                                                                                                                                            CharacteristicType.AverageRemoteness,
                                                                                                                                            CharacteristicType.GCRatio,
                                                                                                                                            CharacteristicType.GCSkew,
                                                                                                                                            CharacteristicType.GCToATRatio,
                                                                                                                                            CharacteristicType.IdentificationInformation,
                                                                                                                                            CharacteristicType.Length,
                                                                                                                                            CharacteristicType.MKSkew,
                                                                                                                                            CharacteristicType.RYSkew,
                                                                                                                                            CharacteristicType.SWSkew
                                                                                                                                          });

        /// <summary>
        /// The characteristic type.
        /// </summary>
        public enum CharacteristicType : int
        {
            /// <summary>
            /// The alphabet cardinality.
            /// </summary>
            AlphabetCardinality = 1,

            /// <summary>
            /// The average remoteness.
            /// </summary>
            AverageRemoteness = 3,

            /// <summary>
            /// The identification information.
            /// </summary>
            IdentificationInformation = 10,

            /// <summary>
            /// The length.
            /// </summary>
            Length = 12,

            /// <summary>
            /// The GC ratio.
            /// </summary>
            GCRatio = 33,

            /// <summary>
            /// The GC skew.
            /// </summary>
            GCSkew = 34,

            /// <summary>
            /// The AT skew.
            /// </summary>
            ATSkew = 35,

            /// <summary>
            /// The GC to AT ratio.
            /// </summary>
            GCToATRatio = 36,

            /// <summary>
            /// The MK skew.
            /// </summary>
            MKSkew = 37,

            /// <summary>
            /// The RY skew.
            /// </summary>
            RYSkew = 38,

            /// <summary>
            /// The SW skew.
            /// </summary>
            SWSkew = 39
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
        /// Notations elements of which will not change.
        /// </summary>
        public static readonly List<Notation> StaticNotations = new List<Notation> 
        { 
            Notation.Nucleotides, 
            Notation.Triplets, 
            Notation.AminoAcids, 
            Notation.Letters 
        };

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
            public const int MitochondrionRibosomalRNA = 12;

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

            /// <summary>
            /// The stem loop.
            /// </summary>
            public const int StemLoop = 25;

            /// <summary>
            /// The displacement loop.
            /// </summary>
            public const int DisplacementLoop = 26;

            /// <summary>
            /// The diversity segment.
            /// </summary>
            public const int DiversitySegment = 27;

            /// <summary>
            /// The mobile element.
            /// </summary>
            public const int MobileElement = 28;

            /// <summary>
            /// The variation.
            /// </summary>
            public const int Variation = 29;

            /// <summary>
            /// The protein bind.
            /// </summary>
            public const int ProteinBind = 30;

            /// <summary>
            /// The mature peptid.
            /// </summary>
            public const int MaturePeptid = 31;

            /// <summary>
            /// The miscellaneous difference.
            /// </summary>
            public const int MiscellaneousDifference = 32;

            /// <summary>
            /// The non coding gene.
            /// </summary>
            public const int Gene = 33;

            /// <summary>
            /// 3'UTR end.
            /// </summary>
            public const int ThreeEnd = 34;

            /// <summary>
            /// 5'UTR end.
            /// </summary>
            public const int FiveEnd = 35;

            /// <summary>
            /// Primer bind site.
            /// </summary>
            public const int PrimerBind = 36;

            /// <summary>
            /// The plastid.
            /// </summary>
            public const int Plastid = 37;
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
