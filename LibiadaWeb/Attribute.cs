namespace LibiadaWeb
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The attribute.
    /// </summary>
    public enum Attribute : byte
    {
        /// <summary>
        /// GenBank id.
        /// </summary>
        [Display(Name = "db_xref")]
        DbXref = 1,

        /// <summary>
        /// The protein id.
        /// </summary>
        [Display(Name = "protein_id")]
        ProteinId = 2,

        /// <summary>
        /// The complement flag.
        /// </summary>
        [Display(Name = "complement")]
        Complement = 3,

        /// <summary>
        /// The complement join flag.
        /// </summary>
        [Display(Name = "complement_join")]
        ComplementJoin = 4,

        /// <summary>
        /// The product name.
        /// </summary>
        [Display(Name = "product")]
        Product = 5,

        /// <summary>
        /// The note.
        /// </summary>
        [Display(Name = "note")]
        Note = 6,

        /// <summary>
        /// The codon start position (1, 2 or 3).
        /// </summary>
        [Display(Name = "codon_start")]
        CodonStart = 7,

        /// <summary>
        /// The translation table for gene.
        /// </summary>
        [Display(Name = "transl_table")]
        TranslationTable = 8,

        /// <summary>
        /// The inference.
        /// </summary>
        [Display(Name = "inference")]
        Inference = 9,

        /// <summary>
        /// The repeat type.
        /// </summary>
        [Display(Name = "rpt_type")]
        RepeatType = 10,

        /// <summary>
        /// The locus tag.
        /// </summary>
        [Display(Name = "locus_tag")]
        LocusTag = 11,

        /// <summary>
        /// The old locus tag.
        /// </summary>
        [Display(Name = "old_locus_tag")]
        OldLocusTag = 12,

        /// <summary>
        /// The gene.
        /// </summary>
        [Display(Name = "gene")]
        Gene = 13,

        /// <summary>
        /// The anticodon.
        /// </summary>
        [Display(Name = "anticodon")]
        Anticodon = 14,

        /// <summary>
        /// The enzyme commission number.
        /// </summary>
        [Display(Name = "EC_number")]
        EcNumber = 15,

        /// <summary>
        /// The exception in annotation or translation.
        /// </summary>
        [Display(Name = "exception")]
        Exception = 16,

        /// <summary>
        /// The gene synonym.
        /// </summary>
        [Display(Name = "gene_synonym")]
        GeneSynonym = 17,

        /// <summary>
        /// The pseudo.
        /// </summary>
        [Display(Name = "pseudo")]
        Pseudo = 18,

        /// <summary>
        /// The ncRNA class.
        /// </summary>
        [Display(Name = "ncRNA_class")]
        NcRnaClass = 19,

        /// <summary>
        /// The standard name.
        /// </summary>
        [Display(Name = "standard_name")]
        StandardName = 20,

        /// <summary>
        /// The repeat family.
        /// </summary>
        [Display(Name = "rpt_family")]
        RepeatFamily = 21,

        /// <summary>
        /// The direction.
        /// </summary>
        [Display(Name = "direction")]
        Direction = 22,

        /// <summary>
        /// The ribosomal slippage.
        /// </summary>
        [Display(Name = "ribosomal_slippage")]
        RibosomalSlippage = 23,

        /// <summary>
        /// The partial.
        /// </summary>
        [Display(Name = "partial")]
        Partial = 24,

        /// <summary>
        /// The codon recognized.
        /// </summary>
        [Display(Name = "codon_recognized")]
        CodonRecognized = 25,

        /// <summary>
        /// The bound moiety.
        /// </summary>
        [Display(Name = "bound_moiety")]
        BoundMoiety = 26,

        /// <summary>
        /// The repeat unit range.
        /// </summary>
        [Display(Name = "rpt_unit_range")]
        RepeatUnitRange = 27,

        /// <summary>
        /// The repeat unit sequence.
        /// </summary>
        [Display(Name = "rpt_unit_seq")]
        RepeatUnitSequence = 28,

        /// <summary>
        /// The function.
        /// </summary>
        [Display(Name = "function")]
        Function = 29,

        /// <summary>
        /// The translational exception.
        /// </summary>
        [Display(Name = "transl_except")]
        TranslationException = 30,

        /// <summary>
        /// The pseudogene.
        /// </summary>
        [Display(Name = "pseudogene")]
        Pseudogene = 31,

        /// <summary>
        /// The mobile element type.
        /// </summary>
        [Display(Name = "mobile_element_type")]
        MobileElementType = 32,

        /// <summary>
        /// The experiment.
        /// </summary>
        [Display(Name = "experiment")]
        Experiment = 33,

        /// <summary>
        /// The citation.
        /// </summary>
        [Display(Name = "citation")]
        Citation = 34,

        /// <summary>
        /// The regulatory class.
        /// </summary>
        [Display(Name = "regulatory_class")]
        RegulatoryClass = 35,

        /// <summary>
        /// The artificial location.
        /// </summary>
        [Display(Name = "artificial_location")]
        ArtificialLocation = 36,

        /// <summary>
        /// The proviral.
        /// </summary>
        [Display(Name = "proviral")]
        Proviral = 37,

        /// <summary>
        /// The operon.
        /// </summary>
        [Display(Name = "operon")]
        Operon = 38,

        /// <summary>
        /// The number.
        /// </summary>
        [Display(Name = "number")]
        Number = 39,

        /// <summary>
        /// The replace.
        /// </summary>
        [Display(Name = "replace")]
        Replace = 40,

        /// <summary>
        /// The reference details of an existing public INSD entry to which a comparison is made.
        /// </summary>
        [Display(Name = "compare")]
        Compare = 41,

        /// <summary>
        /// The name of the allele for the given gene.
        /// </summary>
        [Display(Name = "allele")]
        Allele = 42,

        /// <summary>
        /// Indicates that exons from two RNA molecules are ligated in intermolecular reaction to form mature RNA.
        /// </summary>
        [Display(Name = "trans_splicing")]
        TransSplicing = 43,

        /// <summary>
        /// Frequency of the occurrence of a feature.
        /// </summary>
        [Display(Name = "frequency")]
        Frequency = 44,

        /// <summary>
        /// Undocumented.
        /// </summary>
        [Display(Name = "GO_function")]
        GOFunction = 45,

        /// <summary>
        /// Undocumented.
        /// </summary>
        [Display(Name = "GO_component")]
        GOComponent = 46,

        /// <summary>
        /// Undocumented.
        /// </summary>
        [Display(Name = "GO_process")]
        GOProcess = 47,

        /// <summary>
        /// Satellite.
        /// </summary>
        [Display(Name = "satellite")]
        Satellite = 48
    }
}
