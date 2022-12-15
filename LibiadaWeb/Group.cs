namespace LibiadaWeb
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using LibiadaWeb.Attributes;

    /// <summary>
    /// The group.
    /// </summary>
    public enum Group : byte
    {
        /// <summary>
        /// The bacteria.
        /// </summary>
        [Display(Name = "Bacteria")]
        [Description("Subgroup of procariotes")]
        [Nature(Nature.Genetic)]
        Bacteria = 1,

        /// <summary>
        /// The classical music.
        /// </summary>
        [Display(Name = "Classical music")]
        [Description("Or any?")]
        [Nature(Nature.Music)]
        ClassicalMusic = 2,

        /// <summary>
        /// The classical literature.
        /// </summary>
        [Display(Name = "Classical literature")]
        [Description("Not poetry")]
        [Nature(Nature.Literature)]
        ClassicalLiterature = 3,

        /// <summary>
        /// The classical literature.
        /// </summary>
        [Display(Name = "Observation data")]
        [Description("Numerical data")]
        [Nature(Nature.MeasurementData)]
        ObservationData = 4,

        /// <summary>
        /// The virus.
        /// </summary>
        [Display(Name = "Virus")]
        [Description("Virus, viroid or bacteriophage")]
        [Nature(Nature.Genetic)]
        Virus = 5,

        /// <summary>
        /// The eucariote.
        /// </summary>
        [Display(Name = "Eucariote")]
        [Description("Eucariote")]
        [Nature(Nature.Genetic)]
        Eucariote = 6,

        /// <summary>
        /// The Picture.
        /// </summary>
        [Display(Name = "Painting")]
        [Description("Painting")]
        [Nature(Nature.Image)]
        Painting = 7,

        /// <summary>
        /// The Photo.
        /// </summary>
        [Display(Name = "Photo")]
        [Description("Photo")]
        [Nature(Nature.Image)]
        Photo = 8,

        /// <summary>
        /// The Photo.
        /// </summary>
        [Display(Name = "Picture")]
        [Description("Picture")]
        [Nature(Nature.Image)]
        Picture = 9,

        /// <summary>
        /// The Archaea.
        /// </summary>
        [Display(Name = "Archaea")]
        [Description("Domain of single-celled organisms")]
        [Nature(Nature.Genetic)]
        Archaea = 10
    }
}
