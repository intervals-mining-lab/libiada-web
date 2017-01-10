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
    }
}
