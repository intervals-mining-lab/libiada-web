namespace LibiadaWeb
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using LibiadaWeb.Attributes;

    /// <summary>
    /// The notation.
    /// </summary>
    public enum Notation : byte
    {
        /// <summary>
        /// Nucleotide notation.
        /// </summary>
        [Display(Name = "Nucleotides")]
        [Description("Basic elements of nucleic acids")]
        [Nature(Nature.Genetic)]
        Nucleotides = 1,

        /// <summary>
        /// The notation triplet.
        /// </summary>
        [Display(Name = "Triplets")]
        [Description("Codons, groups of 3 nucleotides")]
        [Nature(Nature.Genetic)]
        Triplets = 2,

        /// <summary>
        /// Amino acid notation.
        /// </summary>
        [Display(Name = "Amino acids")]
        [Description("Basic components of peptides")]
        [Nature(Nature.Genetic)]
        AminoAcids = 3,

        /// <summary>
        /// Segmented dna notation.
        /// </summary>
        [Display(Name = "Genetic words")]
        [Description("Result of segmentation - joined sequence of nucleotides")]
        [Nature(Nature.Genetic)]
        GeneticWords = 4,

        /// <summary>
        /// Words notation.
        /// </summary>
        [Display(Name = "Normalized words")]
        [Description("Words in normalized notation")]
        [Nature(Nature.Literature)]
        NormalizedWords = 5,

        /// <summary>
        /// Fmotifs notation.
        /// </summary>
        [Display(Name = "Formal motifs")]
        [Description("Joined sequence of notes - result of segmentation of musical composition with Boroda algorithm")]
        [Nature(Nature.Music)]
        FormalMotifs = 6,

        /// <summary>
        /// Measures notation.
        /// </summary>
        [Display(Name = "Measures")]
        [Description("Segmented by blocks of notes musical composition")]
        [Nature(Nature.Music)]
        Measures = 7,

        /// <summary>
        /// Notes notation.
        /// </summary>
        [Display(Name = "Notes")]
        [Description("Basic elements of musical composition")]
        [Nature(Nature.Music)]
        Notes = 8,

        /// <summary>
        /// Letters The notation.
        /// </summary>
        [Display(Name = "Letters")]
        [Description("Basic elements of literary work")]
        [Nature(Nature.Literature)]
        Letters = 9,

        /// <summary>
        /// The integer values.
        /// </summary>
        [Display(Name = "Integer values")]
        [Description("Numeric values of measured parameter")]
        [Nature(Nature.MeasurementData)]
        IntegerValues = 10,

        /// <summary>
        /// The consonance notation.
        /// </summary>
        [Display(Name = "Consonance notation")]
        [Description("Phonemes segmented into short sequences")]
        [Nature(Nature.Literature)]
        Consonance = 11,

        /// <summary>
        /// The consonance notation.
        /// </summary>
        [Display(Name = "Phonemes")]
        [Description("Literature texts and poems segmented into phonemes")]
        [Nature(Nature.Literature)]
        Phonemes = 12,

        /// <summary>
        /// The image pixels notation.
        /// </summary>
        [Display(Name = "Pixels")]
        [Description("Images' pixels")]
        [Nature(Nature.Image)]
        Pixels = 13,
    }
}
