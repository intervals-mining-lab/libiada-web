namespace LibiadaWeb
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using Microsoft.Ajax.Utilities;

    /// <summary>
    /// Nature of sequence, elements, etc.
    /// </summary>
    public enum Nature : byte
    {
        /// <summary>
        /// Genetic texts, nucleotides, codons, aminoacids, segmented genetic words, etc.
        /// </summary>
        [Display(Name = "Genetic")]
        [Description("Genetic texts, nucleotides, codons, aminoacids, segmented genetic words, etc.")]
        Genetic = 1,

        /// <summary>
        /// Musical compositions, note, measures, formal motives, etc.
        /// </summary>
        [Display(Name = "Music")]
        [Description("Musical compositions, note, measures, formal motives, etc.")]
        Music = 2,

        /// <summary>
        /// Literary works, letters, words, etc.
        /// </summary>
        [Display(Name = "Literary works, letters, words, etc.")]
        [Description("Link")]
        Literature = 3,

        /// <summary>
        /// Ordered arrays of measurement data, numbers, etc.
        /// </summary>
        [Display(Name = "Measurement data sequences")]
        [Description("Ordered arrays of measurement data, numbers, etc.")]
        MeasurementData = 4
    }
}
