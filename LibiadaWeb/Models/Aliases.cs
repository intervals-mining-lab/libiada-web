namespace LibiadaWeb.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using LibiadaCore.Core;
    using LibiadaCore.Core.ArrangementManagers;
    using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;

    /// <summary>
    /// The aliases.
    /// </summary>
    public static class Aliases
    {
        /// <summary>
        /// The user available arrangement types.
        /// </summary>
        public static readonly ReadOnlyCollection<ArrangementType> UserAvailableArrangementTypes = new ReadOnlyCollection<ArrangementType>(new List<ArrangementType>
        {
            ArrangementType.Intervals
        });

        /// <summary>
        /// The user available links.
        /// </summary>
        public static readonly ReadOnlyCollection<Link> UserAvailableLinks = new ReadOnlyCollection<Link>(new List<Link>
        {
            Link.NotApplied,
            Link.Start,
            Link.Cycle
        });

        /// <summary>
        /// The user available characteristics.
        /// </summary>
        public static readonly ReadOnlyCollection<FullCharacteristic> UserAvailableFullCharacteristics = new ReadOnlyCollection<FullCharacteristic>(new List<FullCharacteristic>
        {
            FullCharacteristic.ATSkew,
            FullCharacteristic.AlphabetCardinality,
            FullCharacteristic.AverageRemoteness,
            FullCharacteristic.GCRatio,
            FullCharacteristic.GCSkew,
            FullCharacteristic.GCToATRatio,
            FullCharacteristic.IdentificationInformation,
            FullCharacteristic.Length,
            FullCharacteristic.MKSkew,
            FullCharacteristic.RYSkew,
            FullCharacteristic.SWSkew
        });

        /// <summary>
        /// The user available congeneric characteristics.
        /// </summary>
        public static readonly ReadOnlyCollection<CongenericCharacteristic> UserAvailableCongenericCharacteristics = new ReadOnlyCollection<CongenericCharacteristic>(new List<CongenericCharacteristic>
        {
            CongenericCharacteristic.AverageRemoteness,
            CongenericCharacteristic.IdentificationInformation,
            CongenericCharacteristic.Length
        });

        /// <summary>
        /// The user available accordance characteristics.
        /// </summary>
        public static readonly ReadOnlyCollection<AccordanceCharacteristic> UserAvailableAccordanceCharacteristics = new ReadOnlyCollection<AccordanceCharacteristic>(new List<AccordanceCharacteristic>
        {
            AccordanceCharacteristic.PartialComplianceDegree,
            AccordanceCharacteristic.MutualComplianceDegree
        });

        /// <summary>
        /// The user available binary characteristics.
        /// </summary>
        public static readonly ReadOnlyCollection<BinaryCharacteristic> UserAvailableBinaryCharacteristics = new ReadOnlyCollection<BinaryCharacteristic>(new List<BinaryCharacteristic>
        {
            BinaryCharacteristic.GeometricMean,
            BinaryCharacteristic.Redundancy,
            BinaryCharacteristic.InvolvedPartialDependenceCoefficient,
            BinaryCharacteristic.PartialDependenceCoefficient,
            BinaryCharacteristic.NormalizedPartialDependenceCoefficient,
            BinaryCharacteristic.MutualDependenceCoefficient,
        });

        /// <summary>
        /// Notations elements of which will not change.
        /// </summary>
        public static readonly ReadOnlyCollection<Notation> StaticNotations = new ReadOnlyCollection<Notation>(new List<Notation>
        {
            Notation.Nucleotides,
            Notation.Triplets,
            Notation.AminoAcids,
            Notation.Letters
        });
    }
}
