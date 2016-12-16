namespace LibiadaWeb
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

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
        /// The plasmid.
        /// </summary>
        [Display(Name = "Plasmid")]
        [Description("")]
        [Nature(Nature.Genetic)]
        Plasmid = 5,

        /// <summary>
        /// The mitochondrion genome.
        /// </summary>
        [Display(Name = "Mitochondrion genome")]
        [Description("")]
        [Nature(Nature.Genetic)]
        MitochondrionGenome = 6,

        /// <summary>
        /// The chloroplast genome.
        /// </summary>
        [Display(Name = "Chloroplast genome")]
        [Description("")]
        [Nature(Nature.Genetic)]
        ChloroplastGenome = 7,

        /// <summary>
        /// The rRNA 16s.
        /// </summary>
        [Display(Name = "16S ribosomal RNA")]
        [Description("")]
        [Nature(Nature.Genetic)]
        RRNA16S = 8,

        /// <summary>
        /// The rRNA 18s.
        /// </summary>
        [Display(Name = "18S ribosomal RNA")]
        [Description("")]
        [Nature(Nature.Genetic)]
        RRNA18S = 9,

        /// <summary>
        /// The mitochondrion 16s rRNA.
        /// </summary>
        [Display(Name = "Mitochondrion 16S ribosomal RNA")]
        [Description("")]
        [Nature(Nature.Genetic)]
        Mitochondrion16SRRNA = 10,

        /// <summary>
        /// Complete plastid genome.
        /// </summary>
        [Display(Name = "Plastid")]
        [Description("")]
        [Nature(Nature.Genetic)]
        Plastid = 11
    }
}