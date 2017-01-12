namespace LibiadaWeb
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using LibiadaWeb.Attributes;

    /// <summary>
    /// The sequence type.
    /// </summary>
    public enum SequenceType : byte
    {
        /// <summary>
        /// The complete genome.
        /// </summary>
        [Display(Name = "Complete genome")]
        [Description("Complete genetic sequence")]
        [Nature(Nature.Genetic)]
        CompleteGenome = 1,

        /// <summary>
        /// The complete musical composition.
        /// </summary>
        [Display(Name = "Complete musical composition")]
        [Description("Complete piece of music")]
        [Nature(Nature.Music)]
        CompleteMusicalComposition = 2,

        /// <summary>
        /// The complete text.
        /// </summary>
        [Display(Name = "Complete text")]
        [Description("Complete literary work")]
        [Nature(Nature.Literature)]
        CompleteText = 3,

        /// <summary>
        /// The complete numeric sequence.
        /// </summary>
        [Display(Name = "Complete numeric sequence")]
        [Description("Full sequence of measured values")]
        [Nature(Nature.MeasurementData)]
        CompleteNumericSequence = 4,

        /// <summary>
        /// Complete plasmid sequence.
        /// </summary>
        [Display(Name = "Plasmid")]
        [Description("Complete plasmid sequence")]
        [Nature(Nature.Genetic)]
        Plasmid = 5,

        /// <summary>
        /// Complete mitochondrion genome.
        /// </summary>
        [Display(Name = "Mitochondrion genome")]
        [Description("Complete mitochondrion genome")]
        [Nature(Nature.Genetic)]
        MitochondrionGenome = 6,

        /// <summary>
        /// Complete chloroplast genome.
        /// </summary>
        [Display(Name = "Chloroplast genome")]
        [Description("Complete chloroplast genome")]
        [Nature(Nature.Genetic)]
        ChloroplastGenome = 7,

        /// <summary>
        /// The rRNA 16s.
        /// </summary>
        [Display(Name = "16S ribosomal RNA")]
        [Description("16S ribosomal RNA of procariote")]
        [Nature(Nature.Genetic)]
        RRNA16S = 8,

        /// <summary>
        /// The rRNA 18s.
        /// </summary>
        [Display(Name = "18S ribosomal RNA")]
        [Description("18S ribosomal RNA of eucariote")]
        [Nature(Nature.Genetic)]
        RRNA18S = 9,

        /// <summary>
        /// The mitochondrion 16s rRNA.
        /// </summary>
        [Display(Name = "Mitochondrion 16S ribosomal RNA")]
        [Description("16S ribosomal RNA from mitochondrion genome")]
        [Nature(Nature.Genetic)]
        Mitochondrion16SRRNA = 10,

        /// <summary>
        /// Complete plastid genome.
        /// </summary>
        [Display(Name = "Plastid")]
        [Description("Complete plastid genome")]
        [Nature(Nature.Genetic)]
        Plastid = 11
    }
}
