using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibiadaWeb
{
    public enum SequenceType : byte
    {
        [Display(Name = "Complete genome")]
        [Description("Complete genetic sequence")]
        [Nature(Nature.Genetic)]
        CompleteGenome = 1,

        [Display(Name = "Complete musical composition")]
        [Description("Complete piece of music")]
        [Nature(Nature.Music)]
        CompleteMusicalComposition = 2,

        [Display(Name = "Complete text")]
        [Description("Complete literary work")]
        [Nature(Nature.Literature)]
        CompleteText = 3,

        [Display(Name = "Complete genome")]
        [Description("Full sequence of measured values")]
        [Nature(Nature.MeasurementData)]
        CompleteNumericSequence = 4,

        [Display(Name = "Plasmid")]
        [Description("")]
        [Nature(Nature.Genetic)]
        Plasmid = 5,

        [Display(Name = "Mitochondrion genome")]
        [Description("")]
        [Nature(Nature.Genetic)]
        MitochondrionGenome = 6,

        [Display(Name = "Chloroplast genome")]
        [Description("")]
        [Nature(Nature.Genetic)]
        ChloroplastGenome = 7,

        [Display(Name = "16S ribosomal RNA")]
        [Description("")]
        [Nature(Nature.Genetic)]
        RRNA16S = 8,

        [Display(Name = "18S ribosomal RNA")]
        [Description("")]
        [Nature(Nature.Genetic)]
        RRNA18S = 9,

        [Display(Name = "Mitochondrion 16S ribosomal RNA")]
        [Description("")]
        [Nature(Nature.Genetic)]
        Mitochondrion16SRRNA = 10
    }
}