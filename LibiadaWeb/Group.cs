namespace LibiadaWeb
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The group.
    /// </summary>
    public enum Group : byte
    {
        /// <summary>
        /// The bacteria.
        /// </summary>
        [Display(Name = "Bacteria")]
        [Description("")]
        [Nature(Nature.Genetic)]
        Bacteria = 1,

        /// <summary>
        /// The virus.
        /// </summary>
        [Display(Name = "Virus")]
        [Description("")]
        [Nature(Nature.Genetic)]
        Virus = 2,

        /// <summary>
        /// The eucariote.
        /// </summary>
        [Display(Name = "Eucariote")]
        [Description("")]
        [Nature(Nature.Genetic)]
        Eucariote = 3,

        /// <summary>
        /// The classical music.
        /// </summary>
        [Display(Name = "Classical music")]
        [Description("")]
        [Nature(Nature.Music)]
        ClassicalMusic = 4,

        /// <summary>
        /// The classical literature.
        /// </summary>
        [Display(Name = "Classical literature")]
        [Description("")]
        [Nature(Nature.Literature)]
        ClassicalLiterature = 5,

        /// <summary>
        /// The classical literature.
        /// </summary>
        [Display(Name = "Observation data")]
        [Description("")]
        [Nature(Nature.MeasurementData)]
        ObservationData = 6
    }
}
